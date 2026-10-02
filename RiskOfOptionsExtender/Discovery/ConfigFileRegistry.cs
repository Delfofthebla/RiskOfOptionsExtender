using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using MonoMod.RuntimeDetour;

namespace RiskOfOptionsExtender.Discovery;

internal static class ConfigFileRegistry
{
    private static readonly List<ConfigFileOrigin> _origins = [];
    private static readonly object _lock = new();
    private static Hook _constructorHook;

    public static IReadOnlyList<ConfigFileOrigin> Origins
    {
        get
        {
            lock (_lock)
                return _origins.ToArray();
        }
    }

    public static void Install()
    {
        var constructor = typeof(ConfigFile).GetConstructor([typeof(string), typeof(bool), typeof(BepInPlugin)]);
        _constructorHook = new Hook(constructor, RecordOrigin);
    }

    private static void RecordOrigin(Action<ConfigFile, string, bool, BepInPlugin> orig, ConfigFile self, string configPath, bool saveOnInit, BepInPlugin ownerMetadata)
    {
        orig(self, configPath, saveOnInit, ownerMetadata);

        var origin = new ConfigFileOrigin(self, ownerMetadata?.GUID, FindCreatingAssembly());
        lock (_lock)
            _origins.Add(origin);
    }

    private static Assembly FindCreatingAssembly()
    {
        foreach (var frame in new StackTrace(2, false).GetFrames() ?? [])
        {
            var assembly = frame.GetMethod()?.DeclaringType?.Assembly;
            if (assembly != null && !IsInfrastructure(assembly))
                return assembly;
        }

        return null;
    }

    private static bool IsInfrastructure(Assembly assembly)
    {
        if (assembly == typeof(ConfigFile).Assembly || assembly == typeof(object).Assembly || assembly == typeof(ConfigFileRegistry).Assembly)
            return true;

        var name = assembly.GetName().Name;
        return name.StartsWith("MonoMod", StringComparison.Ordinal) || name.StartsWith("0Harmony", StringComparison.Ordinal);
    }
}
