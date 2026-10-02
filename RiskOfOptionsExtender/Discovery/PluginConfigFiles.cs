using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using RiskOfOptionsExtender.Generation;

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
        if (ProxyEntries.IsProxyFile(origin.File))
            return;

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

        var type = instance.GetType();
        foreach (var field in type.GetFields(AnyMember))
            AddDistinct(files, ConfigFileOf(field.FieldType, () => field.GetValue(field.IsStatic ? null : instance)));

        foreach (var property in type.GetProperties(AnyMember).Where(property => property.GetIndexParameters().Length == 0 && property.CanRead))
            AddDistinct(files, ConfigFileOf(property.PropertyType, () => property.GetValue(instance)));
    }

    private static ConfigFile ConfigFileOf(Type memberType, Func<object> readValue)
    {
        if (!typeof(ConfigFile).IsAssignableFrom(memberType) && !typeof(ConfigEntryBase).IsAssignableFrom(memberType))
            return null;

        try
        {
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

    private static void AddDistinct(List<ConfigFile> files, ConfigFile file)
    {
        if (file != null && !files.Contains(file))
            files.Add(file);
    }
}
