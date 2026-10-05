using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Discovery;

internal static class ConfigFileRegistry
{
    private const string PatcherAssemblyName = "RiskOfOptionsExtender.Patcher";
    private const string PatcherTypeName = "RiskOfOptionsExtender.Patcher.RiskOfOptionsExtenderPatcher";

    private static Func<IReadOnlyList<(ConfigFile File, string OwnerGuid, Assembly CreatingAssembly)>> _readOrigins = () => [];

    public static IEnumerable<ConfigFileOrigin> Origins
    {
        get
        {
            return _readOrigins().Select(origin => new ConfigFileOrigin(origin.File, origin.OwnerGuid, origin.CreatingAssembly));
        }
    }

    public static void Install()
    {
        if (TryUsePatcher())
            return;

        RiskOfOptionsExtenderPlugin.Log.LogWarning(
            "The RiskOfOptionsExtender patcher isn't running, so config files that earlier-loading mods keep outside their plugin class " +
            "won't get menu options. Reinstalling the mod restores the patcher.");

        ConfigFileWatch.Install();
        _readOrigins = () => ConfigFileWatch.Origins;
    }

    private static bool TryUsePatcher()
    {
        var patcher = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == PatcherAssemblyName)
            ?.GetType(PatcherTypeName);

        if (patcher?.GetProperty("IsWatching")?.GetValue(null) is not true)
            return false;

        var originsProperty = patcher.GetProperty("ConfigFileOrigins");
        if (originsProperty?.PropertyType != typeof(IReadOnlyList<(ConfigFile, string, Assembly)>))
            return false;

        _readOrigins = () => (IReadOnlyList<(ConfigFile, string, Assembly)>)originsProperty.GetValue(null);
        return true;
    }
}
