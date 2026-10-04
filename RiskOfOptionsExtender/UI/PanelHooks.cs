using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Generation;
using RiskOfOptionsExtender.UI.CategoryPicking;
using RiskOfOptionsExtender.UI.ModList;
using RiskOfOptionsExtender.UI.OptionRows;

namespace RiskOfOptionsExtender.UI;

internal static class PanelHooks
{
    private const BindingFlags AnyInstanceMethod = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static Hook _generationHook;
    private static HookSet _modListHooks;
    private static HookSet _optionPageHooks;

    private static ExtenderSettings Settings => RiskOfOptionsExtenderPlugin.Settings;

    public static void Install()
    {
        var panelStart = PanelMethod(nameof(ModOptionPanelController.Start));
        _generationHook = new Hook(panelStart, GenerateBeforePanelStart);

        _modListHooks = new HookSet((panelStart, OrganizeAfterPanelStart));
        _optionPageHooks = new HookSet(
            (PanelMethod(nameof(ModOptionPanelController.LoadModOptionsFromOptionCollection)), OnModLoaded),
            (PanelMethod(nameof(ModOptionPanelController.LoadOptionListFromCategory)), OnCategoryLoaded));

        Settings.ModListTweaks.SettingChanged += (_, _) => SyncModListHooks();
        Settings.HideEmptyMods.SettingChanged += (_, _) => SyncModListHooks();
        Settings.OptionPageTweaks.SettingChanged += (_, _) => SyncOptionPageHooks();

        SyncModListHooks();
        SyncOptionPageHooks();
    }

    private static void SyncModListHooks()
    {
        var enabled = Settings.ModListTweaks.Value || Settings.HideEmptyMods.Value;
        _modListHooks.SetInstalled(enabled);

        foreach (var panel in LivePanels.All)
        {
            if (enabled)
                RunGuarded("organize the mod list", () => ModListOrganizer.AttachOrRefresh(panel));
            else
                RunGuarded("restore the mod list", () => ModListOrganizer.Release(panel));
        }
    }

    private static void SyncOptionPageHooks()
    {
        var enabled = Settings.OptionPageTweaks.Value;
        if (enabled == _optionPageHooks.IsInstalled)
            return;

        _optionPageHooks.SetInstalled(enabled);

        if (enabled)
            ApplyOptionPageTweaksToOpenPanels();
        else
            RemoveOptionPageTweaks();
    }

    private static void ApplyOptionPageTweaksToOpenPanels()
    {
        RunGuarded("add restart notes", RestartNotes.Apply);

        foreach (var panel in LivePanels.All)
        {
            RunGuarded("fit option rows", () => OptionRowFitter.AttachToRows(panel));

            var openModGuid = LivePanels.OpenModGuid(panel);
            if (openModGuid != null)
                RunGuarded("update the category picker", () => CategoryPicker.ShowFor(panel, openModGuid));
        }
    }

    private static void RemoveOptionPageTweaks()
    {
        RunGuarded("remove restart notes", RestartNotes.Remove);
        RunGuarded("remove the category pickers", CategoryPicker.RemoveAll);
        RunGuarded("restore option rows", OptionRowFitter.RevertAll);
    }

    private static MethodInfo PanelMethod(string name)
    {
        return typeof(ModOptionPanelController).GetMethod(name, AnyInstanceMethod);
    }

    private static void GenerateBeforePanelStart(Action<ModOptionPanelController> orig, ModOptionPanelController self)
    {
        LivePanels.Register(self);
        RunGuarded("generate missing options", OptionGeneration.Run);

        if (Settings.OptionPageTweaks.Value)
            RunGuarded("add restart notes", RestartNotes.Apply);

        orig(self);
    }

    private static void OrganizeAfterPanelStart(Action<ModOptionPanelController> orig, ModOptionPanelController self)
    {
        orig(self);
        RunGuarded("organize the mod list", () => ModListOrganizer.AttachOrRefresh(self));
    }

    private static void OnModLoaded(Action<ModOptionPanelController, string> orig, ModOptionPanelController self, string modGuid)
    {
        orig(self, modGuid);
        RunGuarded("update the category picker", () => CategoryPicker.ShowFor(self, modGuid));
    }

    private static void OnCategoryLoaded(Action<ModOptionPanelController, string, int> orig, ModOptionPanelController self, string modGuid, int categoryIndex)
    {
        orig(self, modGuid, categoryIndex);
        RunGuarded("fit option rows", () => OptionRowFitter.AttachToRows(self));
        RunGuarded("sync the category picker", () => CategoryPicker.SyncSelection(self, categoryIndex));
    }

    private static void RunGuarded(string purpose, Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            RiskOfOptionsExtenderPlugin.Log.LogError($"Failed to {purpose}: {exception}");
        }
    }
}
