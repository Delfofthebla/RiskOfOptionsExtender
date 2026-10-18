using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using PluginInfo = BepInEx.PluginInfo;

namespace RiskOfOptionsExtender.Generation;

internal static class OptionGeneratorCoexistence
{
    private static readonly string Guid = "local.option.generator";
    private static readonly string EnabledSection = "Enabled";

    private static bool _announced;

    public static bool Handles(PluginInfo plugin)
    {
        if (!TryGetOptionGenerator(out var optionGenerator))
            return false;

        AnnounceOnce();
        return IsSwitchedOff(optionGenerator, plugin) || plugin.Metadata.GUID == Guid || !DependsOnRiskOfOptions(plugin);
    }

    private static bool TryGetOptionGenerator(out PluginInfo optionGenerator) => Chainloader.PluginInfos.TryGetValue(Guid, out optionGenerator) && optionGenerator.Instance;

    private static bool IsSwitchedOff(PluginInfo optionGenerator, PluginInfo plugin)
        => optionGenerator.Instance.Config.TryGetEntry<bool>(EnabledSection, plugin.Metadata.GUID, out var enabled) && !enabled.Value;

    private static bool DependsOnRiskOfOptions(PluginInfo plugin) => plugin.Dependencies.Any(dependency => dependency.DependencyGUID == RiskOfOptions.PluginInfo.PLUGIN_GUID);

    private static void AnnounceOnce()
    {
        if (_announced)
            return;

        _announced = true;
        RiskOfOptionsExtenderPlugin.Log.LogInfo("OptionGenerator is installed: it fills in mods that don't use Risk Of Options, and this mod fills in the rest.");
    }
}
