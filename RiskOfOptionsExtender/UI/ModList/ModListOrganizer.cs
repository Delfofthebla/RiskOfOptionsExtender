using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RiskOfOptions;
using RiskOfOptions.Components.Panel;
using RiskOfOptions.Components.RuntimePrefabs;
using RiskOfOptionsExtender.Generation;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace RiskOfOptionsExtender.UI.ModList;

internal sealed class ModListOrganizer : MonoBehaviour
{
    private static readonly Regex RichTextTag = new("<.*?>", RegexOptions.Compiled);

    private readonly List<ModListEntry> _entries = [];
    private readonly List<PinToggle> _pins = [];
    private ModOptionsPanelPrefab _panel;
    private HGHeaderNavigationController _navigation;
    private List<HGHeaderNavigationController.Header> _originalHeaders;
    private ModListToolbar _toolbar;
    private string _filter = "";

    private bool TweaksShown => _toolbar;

    private static ExtenderSettings Settings => RiskOfOptionsExtenderPlugin.Settings;

    public static void AttachOrRefresh(ModOptionPanelController panelController)
    {
        var panel = panelController._panel;
        if (panel == null || !panel.ModListPanel)
            return;

        var organizer = panel.ModListPanel.GetComponent<ModListOrganizer>();
        if (!organizer)
        {
            organizer = panel.ModListPanel.AddComponent<ModListOrganizer>();
            organizer.Initialize(panel);
        }

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

    public void SetFilter(string filter)
    {
        _filter = filter?.Trim() ?? "";
        Apply();
    }

    public void Apply()
    {
        var selectedButton = SelectedButton();
        var visibleEntries = SortEntries(_entries.Where(IsVisible)).ToList();
        var visibleSet = new HashSet<ModListEntry>(visibleEntries);

        foreach (var entry in _entries)
            entry.Button.gameObject.SetActive(visibleSet.Contains(entry));

        for (var index = 0; index < visibleEntries.Count; index++)
            visibleEntries[index].Button.transform.SetSiblingIndex(index);

        _navigation.headers = visibleEntries.Select(entry => entry.Header).ToList();
        _navigation.currentHeaderIndex = visibleEntries.FindIndex(entry => entry.Button == selectedButton);
    }

    private void Initialize(ModOptionsPanelPrefab panel)
    {
        _panel = panel;
        _navigation = panel.ModListPanel.GetComponent<HGHeaderNavigationController>();
        _originalHeaders = _navigation.headers.ToList();

        foreach (var header in _originalHeaders)
        {
            if (header.headerButton is ModListButton button)
                _entries.Add(new ModListEntry(button, header, DisplayNameOf(button.modGuid), IsEmpty(button.modGuid)));
        }
    }

    private void Refresh()
    {
        if (Settings.ModListTweaks.Value)
            ShowTweaks();
        else
            HideTweaks();

        Apply();
    }

    private void ShowTweaks()
    {
        if (TweaksShown)
            return;

        _toolbar = ModListToolbar.Create(_panel, this);
        foreach (var entry in _entries)
            _pins.Add(PinToggle.AddTo(entry.Button, this));
    }

    private void HideTweaks()
    {
        if (!TweaksShown)
            return;

        _toolbar.Remove();
        _toolbar = null;
        _filter = "";

        foreach (var pin in _pins)
        {
            if (pin)
                pin.Remove();
        }

        _pins.Clear();
    }

    private void ReleaseList()
    {
        HideTweaks();

        var selectedButton = SelectedButton();
        for (var index = 0; index < _entries.Count; index++)
        {
            _entries[index].Button.gameObject.SetActive(true);
            _entries[index].Button.transform.SetSiblingIndex(index);
        }

        _navigation.headers = _originalHeaders;
        _navigation.currentHeaderIndex = _originalHeaders.FindIndex(header => header.headerButton == selectedButton);
        Destroy(this);
    }

    private void OnEnable()
    {
        Settings.SortOrder.SettingChanged += OnSortOrderChanged;

        if (_navigation)
            Apply();
    }

    private void OnDisable()
    {
        Settings.SortOrder.SettingChanged -= OnSortOrderChanged;
    }

    private void OnSortOrderChanged(object sender, EventArgs args)
    {
        Apply();
    }

    private IEnumerable<ModListEntry> SortEntries(IEnumerable<ModListEntry> entries)
    {
        if (!TweaksShown)
            return entries;

        var pinnedFirst = entries.OrderByDescending(entry => Settings.IsPinned(entry.ModGuid));

        return Settings.SortOrder.Value == ModSortOrder.ReverseAlphabetical
            ? pinnedFirst.ThenByDescending(entry => entry.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            : pinnedFirst.ThenBy(entry => entry.DisplayName, StringComparer.CurrentCultureIgnoreCase);
    }

    private bool IsVisible(ModListEntry entry)
    {
        if (entry.IsEmpty && Settings.HideEmptyMods.Value)
            return false;

        return _filter.Length == 0
            || entry.DisplayName.IndexOf(_filter, StringComparison.CurrentCultureIgnoreCase) >= 0
            || entry.ModGuid.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private MPButton SelectedButton()
    {
        var index = _navigation.currentHeaderIndex;
        var headers = _navigation.headers;
        return index >= 0 && index < headers.Count ? headers[index].headerButton : null;
    }

    private static string DisplayNameOf(string modGuid)
    {
        var name = Language.GetString(ModSettingsManager.OptionCollection[modGuid].NameToken);
        return RichTextTag.Replace(name, "").Trim();
    }

    private static bool IsEmpty(string modGuid)
    {
        return RegisteredEntries.CountOptions(ModSettingsManager.OptionCollection[modGuid]) == 0;
    }
}
