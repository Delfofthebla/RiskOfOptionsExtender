using BepInEx;
using BepInEx.Logging;
using RiskOfOptionsExtender.Discovery;
using RiskOfOptionsExtender.Generation;
using RiskOfOptionsExtender.Resilience;
using RiskOfOptionsExtender.Settings;
using RiskOfOptionsExtender.UI.Features;

namespace RiskOfOptionsExtender;

[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID)]
public class RiskOfOptionsExtenderPlugin : BaseUnityPlugin
{
    public const string GUID = "com.Delfofthebla.RiskOfOptionsExtender";
    public const string NAME = "RiskOfOptionsExtender";
    public const string VERSION = "1.0.1";

    internal static ManualLogSource Log { get; private set; }

    private void Awake()
    {
        Log = Logger;

        new FeatureGuard("config file discovery").Run("start watching for config files", ConfigFileRegistry.Install);

        var settings = new ExtenderSettings(Config);
        OptionGeneration.Initialize(Info, settings);

        new FeatureGuard("this mod's own options page").Run("add this mod's options to the menu", settings.AddToMenu);
        PanelHooks.Install(settings);
    }
}
