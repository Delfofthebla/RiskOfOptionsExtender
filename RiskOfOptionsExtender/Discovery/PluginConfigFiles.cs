using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Discovery;

internal static class PluginConfigFiles
{
    private const BindingFlags AnyMember = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    public static Dictionary<string, List<ConfigFile>> Collect()
    {
        var plugins = Chainloader.PluginInfos.Values.Where(plugin => plugin.Instance).ToList();
        var filesByGuid = plugins.ToDictionary(plugin => plugin.Metadata.GUID, _ => new List<ConfigFile>());
        var guidByAssembly = GuidByAssembly(plugins);

        foreach (var plugin in plugins)
            AddPluginInstanceFiles(plugin, filesByGuid[plugin.Metadata.GUID]);

        foreach (var origin in ConfigFileRegistry.Origins)
            AddRegisteredFile(origin, filesByGuid, guidByAssembly);

        return filesByGuid;
    }

    private static Dictionary<Assembly, string> GuidByAssembly(IEnumerable<PluginInfo> plugins)
    {
        var guidByAssembly = new Dictionary<Assembly, string>();
        foreach (var plugin in plugins)
        {
            var assembly = plugin.Instance.GetType().Assembly;
            if (!guidByAssembly.ContainsKey(assembly))
                guidByAssembly[assembly] = plugin.Metadata.GUID;
        }

        return guidByAssembly;
    }

    private static void AddRegisteredFile(ConfigFileOrigin origin, Dictionary<string, List<ConfigFile>> filesByGuid, Dictionary<Assembly, string> guidByAssembly)
    {
        var guid = origin.OwnerGuid;
        if (guid == null && origin.CreatingAssembly != null)
            guidByAssembly.TryGetValue(origin.CreatingAssembly, out guid);

        if (guid == null || !filesByGuid.TryGetValue(guid, out var files))
            return;

        AddDistinct(files, origin.File);
    }

    private static void AddPluginInstanceFiles(PluginInfo plugin, List<ConfigFile> files)
    {
        var instance = plugin.Instance;
        AddDistinct(files, instance.Config);

        try
        {
            AddMemberFiles(instance, files);
        }
        catch (Exception exception)
        {
            RiskOfOptionsExtenderPlugin.Log.LogDebug($"Could not scan {plugin.Metadata.Name} for config files: {exception.Message}");
        }
    }

    private static void AddMemberFiles(BaseUnityPlugin instance, List<ConfigFile> files)
    {
        var type = instance.GetType();
        foreach (var field in type.GetFields(AnyMember))
            AddDistinct(files, ConfigFileOf(() => field.FieldType, () => field.GetValue(field.IsStatic ? null : instance)));

        foreach (var property in type.GetProperties(AnyMember))
            AddDistinct(files, ConfigFileOf(() => IsPlainGetter(property) ? property.PropertyType : null, () => property.GetValue(instance)));
    }

    private static bool IsPlainGetter(PropertyInfo property)
    {
        return property.CanRead && property.GetIndexParameters().Length == 0;
    }

    // A plugin's members can use types from optional mods that aren't installed, which throws as soon as they're inspected.
    private static ConfigFile ConfigFileOf(Func<Type> memberType, Func<object> readValue)
    {
        try
        {
            if (!IsConfigType(memberType()))
                return null;

            return readValue() switch
            {
                ConfigFile file => file,
                ConfigEntryBase entry => entry.ConfigFile,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static bool IsConfigType(Type type)
    {
        return type != null && (typeof(ConfigFile).IsAssignableFrom(type) || typeof(ConfigEntryBase).IsAssignableFrom(type));
    }

    private static void AddDistinct(List<ConfigFile> files, ConfigFile file)
    {
        if (file != null && !files.Contains(file) && !InternalConfigFiles.Contains(file))
            files.Add(file);
    }
}
