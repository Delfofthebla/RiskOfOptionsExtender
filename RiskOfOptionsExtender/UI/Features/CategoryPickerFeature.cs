using System;
using MonoMod.RuntimeDetour;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Settings;
using RiskOfOptionsExtender.UI.CategoryPicking;

namespace RiskOfOptionsExtender.UI.Features;

internal sealed class CategoryPickerFeature(ExtenderSettings settings) : PanelFeature("the All Tabs picker")
{
    protected override bool IsEnabled => settings.OptionPageTweaks.Value;

    protected override Hook CreateHook()
    {
        return new Hook(PanelMethods.LoadModOptions, ShowPickerAfterModLoaded);
    }

    protected override void ApplyToOpenPanels()
    {
        foreach (var panel in LivePanels.All)
        {
            var openModGuid = LivePanels.OpenModGuid(panel);
            if (openModGuid != null)
                CategoryPicker.ShowFor(panel, openModGuid, Guard);
        }
    }

    protected override void RemoveFromOpenPanels()
    {
        CategoryPicker.RemoveAll();
    }

    private void ShowPickerAfterModLoaded(Action<ModOptionPanelController, string> orig, ModOptionPanelController self, string modGuid)
    {
        orig(self, modGuid);
        LivePanels.Register(self);
        Guard.Run("show the All Tabs picker", () => CategoryPicker.ShowFor(self, modGuid, Guard));
    }
}
