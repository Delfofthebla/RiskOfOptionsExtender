using MonoMod.RuntimeDetour;
using RiskOfOptionsExtender.Resilience;

namespace RiskOfOptionsExtender.UI.Features;

internal abstract class PanelFeature
{
    private readonly string _name;
    private Hook _hook;

    protected PanelFeature(string name)
    {
        _name = name;
        Guard = new FeatureGuard(name, TurnOff);
    }

    protected FeatureGuard Guard { get; }

    protected abstract bool IsEnabled { get; }

    public void Sync()
    {
        if (IsEnabled)
            Guard.Run($"turn on {_name}", TurnOn);
        else
            Guard.Run($"turn off {_name}", TurnOff);
    }

    protected abstract Hook CreateHook();

    protected abstract void ApplyToOpenPanels();

    protected abstract void RemoveFromOpenPanels();

    private void TurnOn()
    {
        _hook ??= CreateHook();
        ApplyToOpenPanels();
    }

    private void TurnOff()
    {
        _hook?.Dispose();
        _hook = null;
        RemoveFromOpenPanels();
    }
}
