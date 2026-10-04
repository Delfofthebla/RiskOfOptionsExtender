using System;
using BepInEx.Bootstrap;

namespace RiskOfOptionsExtender.Resilience;

internal sealed class FeatureGuard(string featureName, Action onTripped = null)
{
    private bool _tripped;

    public void Run(string purpose, Action action)
    {
        if (_tripped)
            return;

        try
        {
            action();
        }
        catch (Exception exception)
        {
            Trip(purpose, exception);
        }
    }

    private void Trip(string purpose, Exception exception)
    {
        _tripped = true;
        RiskOfOptionsExtenderPlugin.Log.LogError(
            $"Failed to {purpose}. Turned off {featureName} until the game restarts. " +
            $"(Risk Of Options Extender {RiskOfOptionsExtenderPlugin.Version}, Risk Of Options {InstalledRiskOfOptionsVersion()})\n{exception}");

        try
        {
            onTripped?.Invoke();
        }
        catch (Exception cleanupException)
        {
            RiskOfOptionsExtenderPlugin.Log.LogError($"Failed to undo the changes of {featureName}: {cleanupException}");
        }
    }

    private static string InstalledRiskOfOptionsVersion()
    {
        return Chainloader.PluginInfos.TryGetValue(RiskOfOptions.PluginInfo.PLUGIN_GUID, out var riskOfOptions)
            ? riskOfOptions.Metadata.Version.ToString()
            : "not loaded";
    }
}
