using System.Reflection;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Discovery;

internal readonly struct ConfigFileOrigin(ConfigFile file, string ownerGuid, Assembly creatingAssembly)
{
    public ConfigFile File { get; } = file;

    public string OwnerGuid { get; } = ownerGuid;

    public Assembly CreatingAssembly { get; } = creatingAssembly;
}
