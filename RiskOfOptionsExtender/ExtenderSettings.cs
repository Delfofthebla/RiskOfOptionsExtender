using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using PluginInfo = BepInEx.PluginInfo;

namespace RiskOfOptionsExtender;

internal sealed class ExtenderSettings
{
    private const string ModListSection = "Mod List";
    private const string FillInSection = "Fill In Missing Options";

    private readonly ConfigFile _config;
    private readonly ConfigEntry<string> _pinnedMods;
    private readonly HashSet<string> _pinnedGuids;
    private readonly Dictionary<string, ConfigEntry<bool>> _fillInToggles = [];
    private bool _staleFillInTogglesRemoved;

    public ConfigEntry<ModSortOrder> SortOrder { get; }

    public ConfigEntry<bool> HideEmptyMods { get; }

    public ConfigEntry<bool> FillInEnabled { get; }

    public ExtenderSettings(ConfigFile config)
    {
        _config = config;

        SortOrder = config.Bind(ModListSection, "Sort Order", ModSortOrder.Alphabetical,
            "Order of the mod list. Pinned mods always come first.");
        HideEmptyMods = config.Bind(ModListSection, "Hide Empty Mods", true,
            "Hide mods that have no options to show.");
        _pinnedMods = config.Bind(ModListSection, "Pinned Mods", "",
            "GUIDs of pinned mods, separated by commas. Pin mods with the star on their row in the mod list.");
        FillInEnabled = config.Bind(FillInSection, "Enabled", true,
            "Add the options mods leave out of this menu. Turn off to never fill in missing options for any mod.");

        _pinnedGuids = [.. _pinnedMods.Value.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(guid => guid.Trim())];

        AddOwnOption(new ChoiceOption(SortOrder));
        AddOwnOption(new CheckBoxOption(HideEmptyMods));
        AddOwnOption(new CheckBoxOption(FillInEnabled, new CheckBoxConfig { restartRequired = true }));
    }

    public bool IsPinned(string modGuid)
    {
        return _pinnedGuids.Contains(modGuid);
    }

    public void TogglePin(string modGuid)
    {
        if (!_pinnedGuids.Remove(modGuid))
            _pinnedGuids.Add(modGuid);

        _pinnedMods.Value = string.Join(",", _pinnedGuids);
    }

    public bool ShouldFillIn(PluginInfo plugin)
    {
        if (!FillInEnabled.Value)
            return false;

        return !_fillInToggles.TryGetValue(plugin.Metadata.GUID, out var toggle) || toggle.Value;
    }

    public void BindFillInToggles(IEnumerable<PluginInfo> pluginsWithMissingOptions)
    {
        var newPlugins = pluginsWithMissingOptions
            .Where(plugin => !_fillInToggles.ContainsKey(plugin.Metadata.GUID))
            .OrderBy(plugin => plugin.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        if (newPlugins.Count == 0 && _staleFillInTogglesRemoved)
            return;

        var saveOnConfigSet = _config.SaveOnConfigSet;
        _config.SaveOnConfigSet = false;

        foreach (var plugin in newPlugins)
            BindFillInToggle(plugin);

        if (!_staleFillInTogglesRemoved)
        {
            RemoveStaleFillInToggles();
            _staleFillInTogglesRemoved = true;
        }

        _config.SaveOnConfigSet = saveOnConfigSet;
        _config.Save();
    }

    private void BindFillInToggle(PluginInfo plugin)
    {
        var guid = plugin.Metadata.GUID;

        ConfigEntry<bool> toggle;
        try
        {
            toggle = _config.Bind(FillInSection, guid, true, $"Add the options \"{plugin.Metadata.Name}\" doesn't put in this menu itself.");
        }
        catch (ArgumentException)
        {
            return;
        }

        _fillInToggles[guid] = toggle;

        var config = new CheckBoxConfig
        {
            name = plugin.Metadata.Name,
            restartRequired = true,
            checkIfDisabled = () => !FillInEnabled.Value
        };
        AddOwnOption(new CheckBoxOption(toggle, config));
    }

    // BepInEx writes entries that were read from the file but never bound back out on every save, so toggles for mods
    // that no longer have missing options would otherwise stay in the file forever.
    private void RemoveStaleFillInToggles()
    {
        var orphanedEntries = typeof(ConfigFile)
            .GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(_config) as Dictionary<ConfigDefinition, string>;

        if (orphanedEntries == null)
            return;

        foreach (var definition in orphanedEntries.Keys.Where(definition => definition.Section == FillInSection).ToList())
            orphanedEntries.Remove(definition);
    }

    private static void AddOwnOption(BaseOption option)
    {
        ModSettingsManager.AddOption(option, RiskOfOptionsExtenderPlugin.Guid, RiskOfOptionsExtenderPlugin.Name);
    }
}
