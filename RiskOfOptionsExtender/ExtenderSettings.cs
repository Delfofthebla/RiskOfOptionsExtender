using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptionsExtender.Discovery;
using PluginInfo = BepInEx.PluginInfo;

namespace RiskOfOptionsExtender;

internal sealed class ExtenderSettings
{
    private const string ModListSection = "Mod List";
    private const string FillInSection = "Fill In Missing Options";
    private const string SkipToggleSection = "Skip";

    private readonly ConfigEntry<string> _pinnedMods;
    private readonly ConfigEntry<string> _skippedMods;
    private readonly HashSet<string> _pinnedGuids;
    private readonly HashSet<string> _skippedGuids;
    private readonly HashSet<string> _guidsWithSkipToggle = [];
    private ConfigFile _skipToggleFile;
    private int _nextSkipToggleKey;

    public ConfigEntry<ModSortOrder> SortOrder { get; }

    public ConfigEntry<bool> HideEmptyMods { get; }

    public ConfigEntry<bool> NeverFillIn { get; }

    public ExtenderSettings(ConfigFile config)
    {
        SortOrder = config.Bind(ModListSection, "Sort Order", ModSortOrder.Alphabetical,
            "Order of the mod list. Pinned mods always come first.");
        HideEmptyMods = config.Bind(ModListSection, "Hide Empty Mods", true,
            "Hide mods that have no options to show.");
        _pinnedMods = config.Bind(ModListSection, "Pinned Mods", "",
            "GUIDs of pinned mods, separated by commas. Pin mods with the star on their row in the mod list.");
        NeverFillIn = config.Bind(FillInSection, "Never Fill In Missing Options", false,
            "Don't add the options mods leave out of this menu, for any mod.");
        _skippedMods = config.Bind(FillInSection, "Skipped Mods", "",
            "GUIDs of mods whose missing options are not added, separated by commas. Set with the \"Skip\" checkboxes in this mod's page.");

        RemoveUnboundEntries(config);

        _pinnedGuids = ParseGuids(_pinnedMods.Value);
        _skippedGuids = ParseGuids(_skippedMods.Value);

        AddOwnOption(new ChoiceOption(SortOrder));
        AddOwnOption(new CheckBoxOption(HideEmptyMods));
        AddOwnOption(new CheckBoxOption(NeverFillIn, new CheckBoxConfig { restartRequired = true }));
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
        return !NeverFillIn.Value && !_skippedGuids.Contains(plugin.Metadata.GUID);
    }

    public void AddSkipToggles(IEnumerable<PluginInfo> pluginsWithMissingOptions)
    {
        var newPlugins = pluginsWithMissingOptions
            .Where(plugin => !_guidsWithSkipToggle.Contains(plugin.Metadata.GUID))
            .OrderBy(plugin => plugin.Metadata.Name, StringComparer.CurrentCultureIgnoreCase);

        foreach (var plugin in newPlugins)
            AddSkipToggle(plugin);
    }

    private void AddSkipToggle(PluginInfo plugin)
    {
        var guid = plugin.Metadata.GUID;
        var name = plugin.Metadata.Name;

        _skipToggleFile ??= UnsavedConfigFiles.Create("SkipToggles");
        var key = (_nextSkipToggleKey++).ToString(CultureInfo.InvariantCulture);
        var toggle = _skipToggleFile.Bind(SkipToggleSection, key, false, $"Don't add the options \"{name}\" leaves out of this menu.");
        toggle.Value = _skippedGuids.Contains(guid);
        toggle.SettingChanged += (_, _) => SetSkipped(guid, toggle.Value);
        _guidsWithSkipToggle.Add(guid);

        var config = new CheckBoxConfig
        {
            name = $"Skip {name}",
            category = FillInSection,
            restartRequired = true,
            checkIfDisabled = () => NeverFillIn.Value
        };
        AddOwnOption(new CheckBoxOption(toggle, config));
    }

    private void SetSkipped(string modGuid, bool skipped)
    {
        if (skipped)
            _skippedGuids.Add(modGuid);
        else
            _skippedGuids.Remove(modGuid);

        _skippedMods.Value = string.Join(",", _skippedGuids.OrderBy(guid => guid, StringComparer.Ordinal));
    }

    // BepInEx keeps entries it read from the file but that were never bound, and writes them back on every save.
    // Every setting this mod uses is bound above, so anything left over is from an older layout.
    private static void RemoveUnboundEntries(ConfigFile config)
    {
        var orphanedEntries = typeof(ConfigFile)
            .GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(config) as Dictionary<ConfigDefinition, string>;

        if (orphanedEntries == null || orphanedEntries.Count == 0)
            return;

        orphanedEntries.Clear();
        config.Save();
    }

    private static HashSet<string> ParseGuids(string commaSeparated)
    {
        return [.. commaSeparated.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(guid => guid.Trim())];
    }

    private static void AddOwnOption(BaseOption option)
    {
        ModSettingsManager.AddOption(option, RiskOfOptionsExtenderPlugin.Guid, RiskOfOptionsExtenderPlugin.Name);
    }
}
