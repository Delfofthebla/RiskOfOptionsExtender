using System;
using MonoMod.RuntimeDetour;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Settings;
using RiskOfOptionsExtender.UI.OptionRows;

namespace RiskOfOptionsExtender.UI.Features;

internal sealed class RestartNotesFeature(ExtenderSettings settings) : PanelFeature("the restart notes")
{
    protected override bool IsEnabled => settings.OptionPageTweaks.Value;

    protected override Hook CreateHook()
    {
        return new Hook(PanelMethods.Start, AddNotesAfterPanelStart);
    }

    protected override void ApplyToOpenPanels()
    {
        RestartNotes.Apply();
    }

    protected override void RemoveFromOpenPanels()
    {
        RestartNotes.Remove();
    }

    private void AddNotesAfterPanelStart(Action<ModOptionPanelController> orig, ModOptionPanelController self)
    {
        orig(self);
        LivePanels.Register(self);
        Guard.Run("add restart notes", RestartNotes.Apply);
    }
}
