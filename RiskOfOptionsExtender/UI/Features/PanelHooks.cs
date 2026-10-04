using System;
using BepInEx.Configuration;
using MonoMod.RuntimeDetour;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Generation;
using RiskOfOptionsExtender.Resilience;
using RiskOfOptionsExtender.Settings;

namespace RiskOfOptionsExtender.UI.Features;

internal static class PanelHooks
{
    private static Hook _startHook;

    public static void Install(ExtenderSettings settings)
    {
        new FeatureGuard("filling in missing options").Run("hook the settings panel's Start", InstallStartHook);

        SyncWith([settings.ModListTweaks, settings.HideEmptyMods], new ModListFeature(settings));
        SyncWith([settings.OptionPageTweaks],
            new RowFittingFeature(settings),
            new RestartIconFeature(settings),
            new RestartNotesFeature(settings),
            new CategoryPickerFeature(settings));
    }

    private static void SyncWith(ConfigEntry<bool>[] settings, params PanelFeature[] features)
    {
        foreach (var setting in settings)
            setting.SettingChanged += (_, _) => SyncAll(features);

        SyncAll(features);
    }

    private static void SyncAll(PanelFeature[] features)
    {
        foreach (var feature in features)
            feature.Sync();
    }

    private static void InstallStartHook()
    {
        _startHook = new Hook(PanelMethods.Start, BeforePanelStart);
    }

    private static void BeforePanelStart(Action<ModOptionPanelController> orig, ModOptionPanelController self)
    {
        LivePanels.Register(self);
        OptionGeneration.Run();
        orig(self);
    }
}
