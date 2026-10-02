using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RiskOfOptions;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Generation;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace RiskOfOptionsExtender.UI.ModList;

internal sealed class ModListOrganizer : MonoBehaviour
{
    private static readonly Regex RichTextTag = new("<.*?>", RegexOptions.Compiled);

    private readonly List<ModListEntry> _entries = [];
    private HGHeaderNavigationController _navigation;
    private string _filter = "";

    private static ExtenderSettings Settings => RiskOfOptionsExtenderPlugin.Settings;

    public static void Attach(ModOptionPanelController panelController)
    {
        var panel = panelController._panel;
        var organizer = panel.ModListPanel.AddComponent<ModListOrganizer>();
        organizer.Initialize(panel.ModListPanel);
        ModListToolbar.Create(panel, organizer);
        organizer.Apply();
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

    private void Initialize(GameObject modListPanel)
    {
        _navigation = modListPanel.GetComponent<HGHeaderNavigationController>();

        foreach (var header in _navigation.headers)
        {
            if (header.headerButton is not ModListButton button)
                continue;

            var entry = new ModListEntry(button, header, DisplayNameOf(button.modGuid), IsEmpty(button.modGuid));
            _entries.Add(entry);
            PinToggle.AddTo(button, this);
        }
    }

    private void OnEnable()
    {
        Settings.SortOrder.SettingChanged += OnListSettingChanged;
        Settings.HideEmptyMods.SettingChanged += OnListSettingChanged;

        if (_navigation)
            Apply();
    }

    private void OnDisable()
    {
        Settings.SortOrder.SettingChanged -= OnListSettingChanged;
        Settings.HideEmptyMods.SettingChanged -= OnListSettingChanged;
    }

    private void OnListSettingChanged(object sender, EventArgs args)
    {
        Apply();
    }

    private IEnumerable<ModListEntry> SortEntries(IEnumerable<ModListEntry> entries)
    {
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
