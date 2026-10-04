using System;
using MonoMod.RuntimeDetour;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Settings;
using RiskOfOptionsExtender.UI.OptionRows;

namespace RiskOfOptionsExtender.UI.Features;

internal sealed class RowFittingFeature(ExtenderSettings settings) : PanelFeature("full option names and aligned checkboxes")
{
    protected override bool IsEnabled => settings.OptionPageTweaks.Value;

    protected override Hook CreateHook()
    {
        return new Hook(PanelMethods.LoadCategory, FitRowsAfterCategoryLoaded);
    }

    protected override void ApplyToOpenPanels()
    {
        foreach (var panel in LivePanels.All)
            OptionRowFitter.AttachToRows(panel, Guard);
    }

    protected override void RemoveFromOpenPanels()
    {
        OptionRowFitter.RevertAll();
    }

    private void FitRowsAfterCategoryLoaded(Action<ModOptionPanelController, string, int> orig, ModOptionPanelController self, string modGuid, int categoryIndex)
    {
        orig(self, modGuid, categoryIndex);
        LivePanels.Register(self);
        Guard.Run("fit option rows", () => OptionRowFitter.AttachToRows(self, Guard));
    }
}
