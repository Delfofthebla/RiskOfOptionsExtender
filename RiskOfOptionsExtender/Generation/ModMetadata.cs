using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using RiskOfOptions;
using RiskOfOptions.Containers;
using RiskOfOptionsExtender.Resilience;
using RiskOfOptionsExtender.UI;
using RoR2;
using UnityEngine;
using Path = System.IO.Path;
using PluginInfo = BepInEx.PluginInfo;

namespace RiskOfOptionsExtender.Generation;

internal static class ModMetadata
{
    private static readonly HashSet<string> _appliedGuids = [];

    public static void Apply(PluginInfo plugin)
    {
        var guid = plugin.Metadata.GUID;
        if (_appliedGuids.Contains(guid) || !ModSettingsManager.OptionCollection.ContainsModGuid(guid))
            return;

        _appliedGuids.Add(guid);
        var collection = ModSettingsManager.OptionCollection[guid];

        try
        {
            if (!collection.icon && !collection.iconPrefab)
                ApplyIcon(plugin, collection);

            if (Language.IsTokenInvalid(collection.DescriptionToken))
                ApplyDescription(plugin, collection);
        }
        catch (Exception exception) when (!IncompatibilityException.Indicates(exception))
        {
            RiskOfOptionsExtenderPlugin.Log.LogDebug($"Could not add the icon or description of {plugin.Metadata.Name}: {exception.Message}");
        }
    }

    private static void ApplyIcon(PluginInfo plugin, OptionCollection collection)
    {
        if (!TryFindPackageFile(plugin, "icon.png", out var path))
            return;

        var sprite = Sprites.FromPng(File.ReadAllBytes(path));
        if (sprite)
            ModSettingsManager.SetModIcon(sprite, collection.ModGuid, collection.ModName);
    }

    private static void ApplyDescription(PluginInfo plugin, OptionCollection collection)
    {
        if (!TryFindPackageFile(plugin, "manifest.json", out var path))
            return;

        var description = JsonUtility.FromJson<ThunderstoreManifest>(File.ReadAllText(path))?.description;
        if (!string.IsNullOrEmpty(description))
            ModSettingsManager.SetModDescription(description, collection.ModGuid, collection.ModName);
    }

    private static bool TryFindPackageFile(PluginInfo plugin, string fileName, out string path)
    {
        path = null;
        var directory = Path.GetDirectoryName(plugin.Location);

        while (!string.IsNullOrEmpty(directory) && !IsPluginsRoot(directory))
        {
            var candidate = Path.Combine(directory, fileName);
            if (File.Exists(candidate))
            {
                path = candidate;
                return true;
            }

            directory = Path.GetDirectoryName(directory);
        }

        return false;
    }

    private static bool IsPluginsRoot(string directory)
    {
        return string.Equals(Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar), Path.GetFullPath(Paths.PluginPath).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
    }

    [Serializable]
    private sealed class ThunderstoreManifest
    {
        public string description = "";
    }
}
