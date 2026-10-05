using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using PluginInfo = BepInEx.PluginInfo;

namespace RiskOfOptionsExtender.Generation;

// OptionGenerator fills in every plugin that doesn't depend on Risk Of Options (and itself), and deletes a plugin's whole
// page while that plugin is switched off in its "Enabled" section. Its prefix and ours run in no guaranteed order, so
// overlapping work would add settings twice; those plugins are left to it instead.
internal static class OptionGeneratorCoexistence
{
    public const string Guid = "local.option.generator";

    private const string EnabledSection = "Enabled";

    private static bool _announced;

    public static bool Handles(PluginInfo plugin)
    {
        if (!TryGetOptionGenerator(out var optionGenerator))
            return false;

        AnnounceOnce();
        return IsSwitchedOff(optionGenerator, plugin) || plugin.Metadata.GUID == Guid || !DependsOnRiskOfOptions(plugin);
    }

    private static bool TryGetOptionGenerator(out PluginInfo optionGenerator)
    {
        return Chainloader.PluginInfos.TryGetValue(Guid, out optionGenerator) && optionGenerator.Instance;
    }

    private static bool IsSwitchedOff(PluginInfo optionGenerator, PluginInfo plugin)
    {
        return optionGenerator.Instance.Config.TryGetEntry<bool>(EnabledSection, plugin.Metadata.GUID, out var enabled) && !enabled.Value;
    }

    private static bool DependsOnRiskOfOptions(PluginInfo plugin)
    {
        return plugin.Dependencies.Any(dependency => dependency.DependencyGUID == RiskOfOptions.PluginInfo.PLUGIN_GUID);
    }

    private static void AnnounceOnce()
    {
        if (_announced)
            return;

        _announced = true;
        RiskOfOptionsExtenderPlugin.Log.LogInfo(
            "OptionGenerator is installed: it fills in mods that don't use Risk Of Options, and this mod fills in the rest.");
    }
}
