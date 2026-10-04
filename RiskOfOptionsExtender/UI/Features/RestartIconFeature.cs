using System;
using MonoMod.RuntimeDetour;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Settings;
using RiskOfOptionsExtender.UI.OptionRows;

namespace RiskOfOptionsExtender.UI.Features;

internal sealed class RestartIconFeature(ExtenderSettings settings) : PanelFeature("the restart icons")
{
    protected override bool IsEnabled => settings.OptionPageTweaks.Value;

    protected override Hook CreateHook()
    {
        return new Hook(PanelMethods.LoadCategory, AddIconsAfterCategoryLoaded);
    }

    protected override void ApplyToOpenPanels()
    {
        foreach (var panel in LivePanels.All)
            RestartIcon.AddToRows(panel);
    }

    protected override void RemoveFromOpenPanels()
    {
        RestartIcon.RemoveAll();
    }

    private void AddIconsAfterCategoryLoaded(Action<ModOptionPanelController, string, int> orig, ModOptionPanelController self, string modGuid, int categoryIndex)
    {
        orig(self, modGuid, categoryIndex);
        LivePanels.Register(self);
        Guard.Run("add restart icons", () => RestartIcon.AddToRows(self));
    }
}
