using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using Mono.Cecil;
using RiskOfOptionsExtender.Discovery;

namespace RiskOfOptionsExtender.Patcher;

public static class RiskOfOptionsExtenderPatcher
{
    // BepInEx only loads a patcher that declares TargetDLLs and Patch; this one patches nothing and only runs Initialize.
    public static IEnumerable<string> TargetDLLs { get; } = [];

    public static bool IsWatching => ConfigFileWatch.IsInstalled;

    public static IReadOnlyList<(ConfigFile File, string OwnerGuid, Assembly CreatingAssembly)> ConfigFileOrigins => ConfigFileWatch.Origins;

    public static void Initialize()
    {
        ConfigFileWatch.Install();
    }

    public static void Patch(AssemblyDefinition assembly)
    {
    }
}
