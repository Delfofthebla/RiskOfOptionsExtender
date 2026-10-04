using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using PluginInfo = BepInEx.PluginInfo;

namespace RiskOfOptionsExtender.Settings;

internal sealed class ExtenderSettings
{
    public const string FillInSection = "Fill In Missing Options";

    private const string ModListSection = "Mod List";
    private const string VisualTweaksSection = "Visual Tweaks";

    public ConfigEntry<ModSortOrder> SortOrder { get; }

    public ConfigEntry<bool> HideEmptyMods { get; }

    public GuidListSetting PinnedMods { get; }

    public ConfigEntry<bool> ModListTweaks { get; }

    public ConfigEntry<bool> OptionPageTweaks { get; }

    public ConfigEntry<bool> NeverFillIn { get; }

    public GuidListSetting SkippedMods { get; }

    public ExtenderSettings(ConfigFile config)
    {
        SortOrder = config.Bind(ModListSection, "Sort Order", ModSortOrder.Alphabetical,
            "Order of the mod list. Pinned mods always come first.");
        HideEmptyMods = config.Bind(ModListSection, "Hide Empty Mods", true,
            "Hide mods that have no options to show.");
        PinnedMods = new GuidListSetting(config.Bind(ModListSection, "Pinned Mods", "",
            "GUIDs of pinned mods, separated by commas. Pin mods with the star on their row in the mod list."));
        ModListTweaks = config.Bind(VisualTweaksSection, "Mod List Tweaks", true,
            "Search box, A-Z / Z-A sorting and pinning in the mod list. When off, this mod leaves Risk Of Options' mod list alone.");
        OptionPageTweaks = config.Bind(VisualTweaksSection, "Option Page Tweaks", true,
            "Restart markers, full setting names, aligned checkboxes and the All Tabs picker on mod option pages. When off, this mod leaves Risk Of Options' option pages alone.");
        NeverFillIn = config.Bind(FillInSection, "Never Fill In Missing Options", false,
            "Don't add the options mods leave out of this menu, for any mod.");
        SkippedMods = new GuidListSetting(config.Bind(FillInSection, "Skipped Mods", "",
            "GUIDs of mods whose missing options are not added, separated by commas. Set with the \"Skip\" checkboxes in this mod's page."));

        RemoveUnboundEntries(config);
    }

    public bool ShouldFillIn(PluginInfo plugin)
    {
        return !NeverFillIn.Value && !SkippedMods.Contains(plugin.Metadata.GUID);
    }

    public void AddToMenu()
    {
        AddOwnOption(new ChoiceOption(SortOrder, new ChoiceConfig { checkIfDisabled = () => !ModListTweaks.Value }));
        AddOwnOption(new CheckBoxOption(HideEmptyMods));
        AddOwnOption(new CheckBoxOption(ModListTweaks));
        AddOwnOption(new CheckBoxOption(OptionPageTweaks));
        AddOwnOption(new CheckBoxOption(NeverFillIn, new CheckBoxConfig { restartRequired = true }));
    }

    public static void AddOwnOption(BaseOption option)
    {
        ModSettingsManager.AddOption(option, RiskOfOptionsExtenderPlugin.Guid, RiskOfOptionsExtenderPlugin.Name);
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
}
