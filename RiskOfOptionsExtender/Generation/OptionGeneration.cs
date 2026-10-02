using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptionsExtender.Discovery;
using PluginInfo = BepInEx.PluginInfo;

namespace RiskOfOptionsExtender.Generation;

internal static class OptionGeneration
{
    private const string FallbackCategory = "General";

    private static readonly HashSet<BaseOption> _generatedOptions = [];
    private static readonly Dictionary<(string ModGuid, string Section), ConfigFile> _sectionFiles = [];
    private static PluginInfo _self;

    public static void Initialize(PluginInfo self)
    {
        _self = self;
    }

    public static void Run()
    {
        var filesByGuid = PluginConfigFiles.Collect();
        var plugins = Chainloader.PluginInfos.Values
            .Where(plugin => plugin.Metadata.GUID != _self.Metadata.GUID && filesByGuid.ContainsKey(plugin.Metadata.GUID))
            .ToList();

        var registered = RegisteredEntries.Collect();
        var pluginsWithMissingOptions = plugins
            .Where(plugin => HasMissingOptions(filesByGuid[plugin.Metadata.GUID], registered))
            .ToList();

        RiskOfOptionsExtenderPlugin.Settings.BindFillInToggles(pluginsWithMissingOptions);

        foreach (var plugin in pluginsWithMissingOptions.Where(RiskOfOptionsExtenderPlugin.Settings.ShouldFillIn))
            FillIn(plugin, filesByGuid[plugin.Metadata.GUID], registered);

        foreach (var plugin in plugins)
            ModMetadata.Apply(plugin);

        ModMetadata.Apply(_self);
    }

    private static bool HasMissingOptions(List<ConfigFile> files, HashSet<ConfigEntryBase> registered)
    {
        return files.Any(file => file.Keys.Any(definition => !registered.Contains(file[definition])));
    }

    private static void FillIn(PluginInfo plugin, List<ConfigFile> files, HashSet<ConfigEntryBase> registered)
    {
        var restartRequired = HasOwnOptions(plugin.Metadata.GUID);
        var hasMultipleFiles = files.Count(file => file.Count > 0) > 1;

        foreach (var file in files)
            FillInFile(plugin, file, hasMultipleFiles, restartRequired, registered);
    }

    private static void FillInFile(PluginInfo plugin, ConfigFile file, bool hasMultipleFiles, bool restartRequired, HashSet<ConfigEntryBase> registered)
    {
        foreach (var definition in file.Keys)
        {
            var entry = file[definition];
            if (registered.Contains(entry))
                continue;

            if (TryAddOption(plugin, file, entry, hasMultipleFiles, restartRequired))
                registered.Add(entry);
        }
    }

    private static bool TryAddOption(PluginInfo plugin, ConfigFile file, ConfigEntryBase entry, bool hasMultipleFiles, bool restartRequired)
    {
        var modGuid = plugin.Metadata.GUID;
        var category = CategoryFor(modGuid, entry.Definition.Section, file, hasMultipleFiles);
        var labels = new OptionLabels(category, entry.Definition.Key, entry.Description.Description);

        var option = OptionFactory.Create(entry, labels, restartRequired);
        if (option == null)
            return false;

        if (!TryClaimUniqueName(modGuid, option, FileLabel(file)))
            return false;

        ModSettingsManager.AddOption(option, modGuid, plugin.Metadata.Name);
        _generatedOptions.Add(option);

        if (hasMultipleFiles)
            CategoryGroups.Record(modGuid, category, FileLabel(file));

        return true;
    }

    private static bool HasOwnOptions(string modGuid)
    {
        if (!ModSettingsManager.OptionCollection.ContainsModGuid(modGuid))
            return false;

        return RegisteredEntries.OptionsOf(ModSettingsManager.OptionCollection[modGuid]).Any(option => !_generatedOptions.Contains(option));
    }

    private static string CategoryFor(string modGuid, string section, ConfigFile file, bool hasMultipleFiles)
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

        return owningFile == file ? section : $"{section} ({FileLabel(file)})";
    }

    private static bool TryClaimUniqueName(string modGuid, BaseOption option, string fileLabel)
    {
        var config = option.GetConfig();
        if (!IsIdentifierTaken(modGuid, option, config.name))
            return true;

        config.name = $"{config.name} ({fileLabel})";
        return !IsIdentifierTaken(modGuid, option, config.name);
    }

    private static bool IsIdentifierTaken(string modGuid, BaseOption option, string name)
    {
        var identifier = $"{modGuid}.{option.GetConfig().category}.{name}.{option.OptionTypeName}".Replace(" ", "_").ToUpper();
        return ModSettingsManager.OptionCollection._identifierModGuidMap.ContainsKey(identifier);
    }

    private static string FileLabel(ConfigFile file)
    {
        return Path.GetFileNameWithoutExtension(file.ConfigFilePath);
    }
}
