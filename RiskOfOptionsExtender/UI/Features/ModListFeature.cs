using System;
using MonoMod.RuntimeDetour;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Resilience;
using RiskOfOptionsExtender.Settings;
using RiskOfOptionsExtender.UI.ModList;

namespace RiskOfOptionsExtender.UI.Features;

internal sealed class ModListFeature : PanelFeature
{
    private readonly ExtenderSettings _settings;
    private readonly ModListGuards _guards;

    public ModListFeature(ExtenderSettings settings) : base("the mod list organizer")
    {
        _settings = settings;
        _guards = new ModListGuards(
            Guard,
            new FeatureGuard("the mod list search and sort bar", ModListOrganizer.RemoveAllToolbars),
            new FeatureGuard("the mod list pin stars", ModListOrganizer.RemoveAllPins));
    }

    protected override bool IsEnabled => _settings.ModListTweaks.Value || _settings.HideEmptyMods.Value;

    protected override Hook CreateHook()
    {
        return new Hook(PanelMethods.Start, OrganizeAfterPanelStart);
    }

    protected override void ApplyToOpenPanels()
    {
        foreach (var panel in LivePanels.All)
            ModListOrganizer.AttachOrRefresh(panel, _settings, _guards);
    }

    protected override void RemoveFromOpenPanels()
    {
        foreach (var panel in LivePanels.All)
            ModListOrganizer.Release(panel);
    }

    private void OrganizeAfterPanelStart(Action<ModOptionPanelController> orig, ModOptionPanelController self)
    {
        orig(self);
        LivePanels.Register(self);
        Guard.Run("organize the mod list", () => ModListOrganizer.AttachOrRefresh(self, _settings, _guards));
    }
}
