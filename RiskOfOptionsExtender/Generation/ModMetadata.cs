using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using RiskOfOptions;
using RiskOfOptions.Containers;
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

        if (!collection.icon && !collection.iconPrefab)
            ApplyIcon(plugin, collection);

        if (Language.IsTokenInvalid(collection.DescriptionToken))
            ApplyDescription(plugin, collection);
    }

    private static void ApplyIcon(PluginInfo plugin, OptionCollection collection)
    {
        if (!TryFindPackageFile(plugin, "icon.png", out var path))
            return;

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(File.ReadAllBytes(path)))
            return;

        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        ModSettingsManager.SetModIcon(sprite, collection.ModGuid, collection.ModName);
    }

    private static void ApplyDescription(PluginInfo plugin, OptionCollection collection)
    {
        if (!TryFindPackageFile(plugin, "manifest.json", out var path))
            return;

        try
        {
            var description = JsonUtility.FromJson<ThunderstoreManifest>(File.ReadAllText(path))?.description;
            if (!string.IsNullOrEmpty(description))
                ModSettingsManager.SetModDescription(description, collection.ModGuid, collection.ModName);
        }
        catch (ArgumentException exception)
        {
            RiskOfOptionsExtenderPlugin.Log.LogDebug($"Could not read {path}: {exception.Message}");
        }
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
