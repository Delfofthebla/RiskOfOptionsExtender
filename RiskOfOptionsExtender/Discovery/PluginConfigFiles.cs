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
    private static readonly BindingFlags AnyMember = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly Dictionary<Type, MemberInfo[]> _configMembersByType = [];

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
        Dictionary<Assembly, string> guidByAssembly = [];
        foreach (var plugin in plugins)
        {
            var assembly = plugin.Instance.GetType().Assembly;
            if (!guidByAssembly.ContainsKey(assembly))
                guidByAssembly[assembly] = plugin.Metadata.GUID;
        }

        return guidByAssembly;
    }

    private static void AddRegisteredFile(
        (ConfigFile File, string OwnerGuid, Assembly CreatingAssembly) origin,
        Dictionary<string, List<ConfigFile>> filesByGuid,
        Dictionary<Assembly, string> guidByAssembly)
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
        foreach (var member in ConfigMembersOf(instance.GetType()))
            AddDistinct(files, ConfigFileIn(member, instance));
    }

    private static MemberInfo[] ConfigMembersOf(Type pluginType)
    {
        if (!_configMembersByType.TryGetValue(pluginType, out var members))
            _configMembersByType[pluginType] = members = [.. pluginType.GetMembers(AnyMember).Where(HoldsConfig)];

        return members;
    }

    // A plugin's members can use types from optional mods that aren't installed, which throws as soon as they're inspected.
    private static bool HoldsConfig(MemberInfo member)
    {
        try
        {
            return IsConfigType(TypeOf(member));
        }
        catch
        {
            return false;
        }
    }

    private static Type TypeOf(MemberInfo member) => member switch
    {
        FieldInfo field => field.FieldType,
        PropertyInfo property when IsPlainGetter(property) => property.PropertyType,
        _ => null
    };

    private static bool IsPlainGetter(PropertyInfo property) => property.CanRead && property.GetIndexParameters().Length == 0;

    private static bool IsConfigType(Type type) => type != null && (typeof(ConfigFile).IsAssignableFrom(type) || typeof(ConfigEntryBase).IsAssignableFrom(type));

    private static ConfigFile ConfigFileIn(MemberInfo member, BaseUnityPlugin instance)
    {
        try
        {
            return ValueOf(member, instance) switch
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

    private static object ValueOf(MemberInfo member, BaseUnityPlugin instance) => member is FieldInfo field
        ? field.GetValue(instance)
        : ((PropertyInfo)member).GetValue(instance);

    private static void AddDistinct(List<ConfigFile> files, ConfigFile file)
    {
        if (file != null && !files.Contains(file) && !InternalConfigFiles.Contains(file))
            files.Add(file);
    }
}
