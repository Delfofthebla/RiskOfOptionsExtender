using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RiskOfOptions;
using RiskOfOptions.Components.Panel;
using RiskOfOptions.Components.RuntimePrefabs;
using RiskOfOptions.Containers;
using RiskOfOptionsExtender.Generation;
using RiskOfOptionsExtender.Settings;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace RiskOfOptionsExtender.UI.ModList;

internal sealed class ModListOrganizer : MonoBehaviour
{
    private static readonly Regex RichTextTag = new("<.*?>", RegexOptions.Compiled);
    private static readonly HashSet<ModListOrganizer> _liveOrganizers = [];

    private readonly List<ModListEntry> _entries = [];
    private readonly List<PinToggle> _pins = [];
    private ModOptionsPanelPrefab _panel;
    private HGHeaderNavigationController _navigation;
    private List<HGHeaderNavigationController.Header> _originalHeaders;
    private ExtenderSettings _settings;
    private ModListGuards _guards;
    private ModListToolbar _toolbar;
    private SuppressedRaycasts _highlightRaycasts;
    private bool _tweaksShown;
    private string _filter = "";

    public static void AttachOrRefresh(ModOptionPanelController panelController, ExtenderSettings settings, ModListGuards guards)
    {
        var panel = panelController._panel;
        if (panel == null || !panel.ModListPanel)
            return;

        var organizer = panel.ModListPanel.GetComponent<ModListOrganizer>();
        if (!organizer)
            organizer = Create(panel, settings, guards);

        organizer.Refresh();
    }

    public static void Release(ModOptionPanelController panelController)
    {
        var panel = panelController._panel;
        if (panel == null || !panel.ModListPanel)
            return;

        var organizer = panel.ModListPanel.GetComponent<ModListOrganizer>();
        if (organizer)
            organizer.ReleaseList();
    }

    public static void RemoveAllToolbars()
    {
        foreach (var organizer in _liveOrganizers.ToList())
        {
            organizer.RemoveToolbar();
            organizer.ReorderGuarded();
        }
    }

    public static void RemoveAllPins()
    {
        foreach (var organizer in _liveOrganizers.ToList())
            organizer.RemovePins();
    }

    public bool IsPinned(string modGuid)
    {
        return _settings.PinnedMods.Contains(modGuid);
    }

    public void TogglePin(string modGuid)
    {
        _settings.PinnedMods.Toggle(modGuid);
        ReorderGuarded();
    }

    private static ModListOrganizer Create(ModOptionsPanelPrefab panel, ExtenderSettings settings, ModListGuards guards)
    {
        var navigation = panel.ModListPanel.GetRequiredComponent<HGHeaderNavigationController>();
        var originalHeaders = navigation.headers.ToList();
        var entries = originalHeaders
            .Where(header => header.headerButton is ModListButton)
            .Select(EntryOf)
            .ToList();

        var organizer = panel.ModListPanel.AddComponent<ModListOrganizer>();
        organizer._panel = panel;
        organizer._navigation = navigation;
        organizer._originalHeaders = originalHeaders;
        organizer._entries.AddRange(entries);
        organizer._settings = settings;
        organizer._guards = guards;
        _liveOrganizers.Add(organizer);

        settings.SortOrder.SettingChanged += organizer.OnSortOrderChanged;
        return organizer;
    }

    private static ModListEntry EntryOf(HGHeaderNavigationController.Header header)
    {
        var button = (ModListButton)header.headerButton;
        var collection = ModSettingsManager.OptionCollection[button.modGuid];
        return new ModListEntry(button, header, DisplayNameOf(collection), RegisteredEntries.CountOptions(collection) == 0);
    }

    private void Refresh()
    {
        if (_settings.ModListTweaks.Value)
            ShowTweaks();
        else
            HideTweaks();

        Reorder();
    }

    private void ShowTweaks()
    {
        _tweaksShown = true;
        _guards.Toolbar.Run("add the mod list search and sort bar", AddToolbar);
        _guards.Pins.Run("add the mod list pin stars", AddPins);
    }

    private void HideTweaks()
    {
        _tweaksShown = false;
        RemoveToolbar();
        RemovePins();
    }

    private void AddToolbar()
    {
        if (!_toolbar)
            _toolbar = ModListToolbar.Create(_panel, _settings.SortOrder, SetFilter);
    }

    private void AddPins()
    {
        if (_pins.Count > 0)
            return;

        _highlightRaycasts = SuppressSelectionHighlightRaycasts();
        foreach (var entry in _entries)
            _pins.Add(PinToggle.AddTo(entry.Button, this));
    }

    private void RemoveToolbar()
    {
        if (_toolbar)
            _toolbar.Remove();

        _toolbar = null;
        _filter = "";
    }

    private void RemovePins()
    {
        foreach (var pin in _pins)
        {
            if (pin)
                pin.Remove();
        }

        _pins.Clear();
        _highlightRaycasts?.Restore();
        _highlightRaycasts = null;
    }

    // The navigation controller moves its highlight onto the selected mod's button as the last child, covering the
    // whole row, so it would take every click meant for that row's pin star.
    private SuppressedRaycasts SuppressSelectionHighlightRaycasts()
    {
        var highlight = _navigation.headerHighlightObject;
        return highlight ? SuppressedRaycasts.Under(highlight) : null;
    }

    private void ReleaseList()
    {
        HideTweaks();

        var selectedButton = _navigation.SelectedButton();
        for (var index = 0; index < _entries.Count; index++)
        {
            _entries[index].Button.gameObject.SetActive(true);
            _entries[index].Button.transform.SetSiblingIndex(index);
        }

        _navigation.headers = _originalHeaders;
        _navigation.currentHeaderIndex = _originalHeaders.FindIndex(header => header.headerButton == selectedButton);
        Destroy(this);
    }

    private void SetFilter(string filter)
    {
        _filter = filter?.Trim() ?? "";
        ReorderGuarded();
    }

    private void ReorderGuarded()
    {
        _guards.Organizer.Run("organize the mod list", Reorder);
    }

    private void Reorder()
    {
        var selectedButton = _navigation.SelectedButton();
        var visibleEntries = SortEntries(_entries.Where(IsVisible)).ToList();
        var visibleSet = new HashSet<ModListEntry>(visibleEntries);

        foreach (var entry in _entries)
            entry.Button.gameObject.SetActive(visibleSet.Contains(entry));

        for (var index = 0; index < visibleEntries.Count; index++)
            visibleEntries[index].Button.transform.SetSiblingIndex(index);

        _navigation.headers = visibleEntries.Select(entry => entry.Header).ToList();
        _navigation.currentHeaderIndex = visibleEntries.FindIndex(entry => entry.Button == selectedButton);
    }

    private void OnEnable()
    {
        if (_navigation)
            ReorderGuarded();
    }

    private void OnDestroy()
    {
        _liveOrganizers.Remove(this);

        if (_settings != null)
            _settings.SortOrder.SettingChanged -= OnSortOrderChanged;
    }

    private void OnSortOrderChanged(object sender, EventArgs args)
    {
        ReorderGuarded();
    }

    private IEnumerable<ModListEntry> SortEntries(IEnumerable<ModListEntry> entries)
    {
        if (!_tweaksShown)
            return entries;

        var pinnedFirst = entries.OrderByDescending(entry => IsPinned(entry.ModGuid));

        return _settings.SortOrder.Value == ModSortOrder.ReverseAlphabetical
            ? pinnedFirst.ThenByDescending(entry => entry.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            : pinnedFirst.ThenBy(entry => entry.DisplayName, StringComparer.CurrentCultureIgnoreCase);
    }

    private bool IsVisible(ModListEntry entry)
    {
        if (entry.IsEmpty && _settings.HideEmptyMods.Value)
            return false;

        return _filter.Length == 0
            || entry.DisplayName.IndexOf(_filter, StringComparison.CurrentCultureIgnoreCase) >= 0
            || entry.ModGuid.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string DisplayNameOf(OptionCollection collection)
    {
        return RichTextTag.Replace(Language.GetString(collection.NameToken), "").Trim();
    }
}
