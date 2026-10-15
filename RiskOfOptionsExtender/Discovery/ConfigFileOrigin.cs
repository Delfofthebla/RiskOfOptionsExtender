using System.Reflection;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Discovery;

internal sealed record ConfigFileOrigin(ConfigFile File, string OwnerGuid, Assembly CreatingAssembly);
