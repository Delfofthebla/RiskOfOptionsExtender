using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptionsExtender.Discovery;
using RiskOfOptionsExtender.Resilience;
using RiskOfOptionsExtender.Settings;
using PluginInfo = BepInEx.PluginInfo;

namespace RiskOfOptionsExtender.Generation;

internal static class OptionGeneration
{
    private const string FallbackCategory = "General";

    private static readonly FeatureGuard _fillInGuard = new("filling in missing options");
    private static readonly FeatureGuard _metadataGuard = new("adding missing mod icons and descriptions");
    private static readonly HashSet<BaseOption> _generatedOptions = [];
    private static readonly HashSet<ConfigEntryBase> _unfillableEntries = [];
    private static readonly HashSet<string> _filledGuids = [];
    private static readonly Dictionary<(string ModGuid, string Section), ConfigFile> _sectionFiles = [];
    private static PluginInfo _self;
    private static ExtenderSettings _settings;
    private static SkipToggles _skipToggles;

    public static void Initialize(PluginInfo self, ExtenderSettings settings)
    {
        _self = self;
        _settings = settings;
        _skipToggles = new SkipToggles(settings);
    }

    public static void Run()
    {
        _fillInGuard.Run("fill in missing options", FillInAll);
        _metadataGuard.Run("add mod icons and descriptions", ApplyMetadata);
    }

    private static void FillInAll()
    {
        var filesByGuid = PluginConfigFiles.Collect();
        var registered = RegisteredEntries.Collect();
        var plugins = Chainloader.PluginInfos.Values
            .Where(plugin => plugin.Metadata.GUID != _self.Metadata.GUID && filesByGuid.ContainsKey(plugin.Metadata.GUID))
            .ToList();

        var pluginsWithFillableEntries = plugins
            .Where(plugin => HasFillableEntries(filesByGuid[plugin.Metadata.GUID], registered))
            .ToList();

        foreach (var plugin in pluginsWithFillableEntries.Where(_settings.ShouldFillIn))
            FillIn(plugin, filesByGuid[plugin.Metadata.GUID], registered);

        var pluginsWithFillIn = pluginsWithFillableEntries.Where(plugin =>
            _filledGuids.Contains(plugin.Metadata.GUID) || HasFillableEntries(filesByGuid[plugin.Metadata.GUID], registered));
        _skipToggles.AddFor(pluginsWithFillIn);
    }

    private static void ApplyMetadata()
    {
        foreach (var plugin in Chainloader.PluginInfos.Values)
            ModMetadata.Apply(plugin);
    }

    private static bool HasFillableEntries(List<ConfigFile> files, HashSet<ConfigEntryBase> registered)
    {
        return files.Any(file => EntriesOf(file).Any(entry => IsFillable(entry, registered)));
    }

    private static IEnumerable<ConfigEntryBase> EntriesOf(ConfigFile file)
    {
        return file.Keys.Select(definition => file[definition]);
    }

    private static bool IsFillable(ConfigEntryBase entry, HashSet<ConfigEntryBase> registered)
    {
        return !registered.Contains(entry) && !_unfillableEntries.Contains(entry) && OptionFactory.CanCreate(entry);
    }

    private static void FillIn(PluginInfo plugin, List<ConfigFile> files, HashSet<ConfigEntryBase> registered)
    {
        var restartRequired = HasOwnOptions(plugin.Metadata.GUID);
        var hasMultipleFiles = files.Count(file => file.Count > 0) > 1;

        foreach (var file in files)
        {
            foreach (var entry in EntriesOf(file).Where(entry => IsFillable(entry, registered)))
                FillInEntry(plugin, file, entry, hasMultipleFiles, restartRequired, registered);
        }
    }

    private static void FillInEntry(PluginInfo plugin, ConfigFile file, ConfigEntryBase entry, bool hasMultipleFiles, bool restartRequired, HashSet<ConfigEntryBase> registered)
    {
        try
        {
            if (TryAddOption(plugin, file, entry, hasMultipleFiles, restartRequired))
                registered.Add(entry);
            else
                _unfillableEntries.Add(entry);
        }
        catch (Exception exception) when (!IncompatibilityException.Indicates(exception))
        {
            _unfillableEntries.Add(entry);
            RiskOfOptionsExtenderPlugin.Log.LogWarning($"Could not add an option for [{entry.Definition.Section}] {entry.Definition.Key} of {plugin.Metadata.Name}: {exception}");
        }
    }

    private static bool TryAddOption(PluginInfo plugin, ConfigFile file, ConfigEntryBase entry, bool hasMultipleFiles, bool restartRequired)
    {
        var modGuid = plugin.Metadata.GUID;
        var fileLabel = Path.GetFileNameWithoutExtension(file.ConfigFilePath);
        var category = CategoryFor(modGuid, entry.Definition.Section, file, fileLabel, hasMultipleFiles);
        var details = new OptionDetails(category, entry.Definition.Key, entry.Description.Description, restartRequired);

        var option = OptionFactory.Create(entry, details);
        if (option == null || !TryClaimUniqueName(modGuid, option, fileLabel))
            return false;

        ModSettingsManager.AddOption(option, modGuid, plugin.Metadata.Name);
        _generatedOptions.Add(option);
        _filledGuids.Add(modGuid);
        OptionIdentifiers.VerifyPrediction(option);

        if (hasMultipleFiles)
            CategoryGroups.Record(modGuid, category, fileLabel);

        return true;
    }

    private static bool HasOwnOptions(string modGuid)
    {
        if (!ModSettingsManager.OptionCollection.ContainsModGuid(modGuid))
            return false;

        return RegisteredEntries.OptionsOf(ModSettingsManager.OptionCollection[modGuid]).Any(option => !_generatedOptions.Contains(option));
    }

    private static string CategoryFor(string modGuid, string section, ConfigFile file, string fileLabel, bool hasMultipleFiles)
    {
        if (string.IsNullOrEmpty(section))
            section = FallbackCategory;

        if (!hasMultipleFiles)
            return section;

        if (!_sectionFiles.TryGetValue((modGuid, section), out var owningFile))
        {
            _sectionFiles[(modGuid, section)] = file;
            return section;
        }

        return owningFile == file ? section : $"{section} ({fileLabel})";
    }

    private static bool TryClaimUniqueName(string modGuid, BaseOption option, string fileLabel)
    {
        var config = option.GetConfig();
        if (!OptionIdentifiers.IsTaken(modGuid, option, config.name))
            return true;

        config.name = $"{config.name} ({fileLabel})";
        return !OptionIdentifiers.IsTaken(modGuid, option, config.name);
    }
}
