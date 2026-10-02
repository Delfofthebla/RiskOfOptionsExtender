using BepInEx;
using BepInEx.Logging;
using RiskOfOptionsExtender.Discovery;
using RiskOfOptionsExtender.Generation;
using RiskOfOptionsExtender.UI;

namespace RiskOfOptionsExtender;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID)]
[BepInIncompatibility(OptionGeneratorGuid)]
public class RiskOfOptionsExtenderPlugin : BaseUnityPlugin
{
    // The leading underscore sorts this plugin ahead of nearly every other in BepInEx's load order,
    // so the config file hook is in place before they create their config files.
    public const string Guid = "_Delfofthebla.RiskOfOptionsExtender";
    public const string Name = "RiskOfOptionsExtender";
    public const string Version = "0.1.0";

    private const string OptionGeneratorGuid = "local.option.generator";

    internal static ManualLogSource Log { get; private set; }

    internal static ExtenderSettings Settings { get; private set; }

    private void Awake()
    {
        Log = Logger;
        ConfigFileRegistry.Install();
        Settings = new ExtenderSettings(Config);

        OptionGeneration.Initialize(Info);
        PanelHooks.Install();
    }
}
