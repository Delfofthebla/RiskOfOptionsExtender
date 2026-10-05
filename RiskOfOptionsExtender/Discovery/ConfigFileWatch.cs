using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using MonoMod.RuntimeDetour;

namespace RiskOfOptionsExtender.Discovery;

internal static class ConfigFileWatch
{
    private static readonly List<(ConfigFile File, string OwnerGuid, Assembly CreatingAssembly)> _origins = [];
    private static readonly object _lock = new();
    private static Hook _constructorHook;
    private static ManualLogSource _log;

    public static bool IsInstalled => _constructorHook != null;

    public static IReadOnlyList<(ConfigFile File, string OwnerGuid, Assembly CreatingAssembly)> Origins
    {
        get
        {
            lock (_lock)
                return _origins.ToArray();
        }
    }

    public static void Install()
    {
        var constructor = typeof(ConfigFile).GetConstructor([typeof(string), typeof(bool), typeof(BepInPlugin)])
            ?? throw new MissingMethodException("BepInEx's ConfigFile(string, bool, BepInPlugin) constructor no longer exists.");

        _constructorHook = new Hook(constructor, RecordOrigin);
    }

    private static void RecordOrigin(Action<ConfigFile, string, bool, BepInPlugin> orig, ConfigFile self, string configPath, bool saveOnInit, BepInPlugin ownerMetadata)
    {
        orig(self, configPath, saveOnInit, ownerMetadata);

        try
        {
            var origin = (self, ownerMetadata?.GUID, FindCreatingAssembly());
            lock (_lock)
                _origins.Add(origin);
        }
        catch (Exception exception)
        {
            _log ??= Logger.CreateLogSource(nameof(ConfigFileWatch));
            _log.LogDebug($"Could not record who created {configPath}: {exception}");
        }
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
        if (assembly == typeof(ConfigFile).Assembly || assembly == typeof(object).Assembly || assembly == typeof(ConfigFileWatch).Assembly)
            return true;

        var name = assembly.GetName().Name;
        return name.StartsWith("MonoMod", StringComparison.Ordinal) || name.StartsWith("0Harmony", StringComparison.Ordinal);
    }
}
