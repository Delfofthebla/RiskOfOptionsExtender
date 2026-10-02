using System;
using System.Collections.Generic;
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

    private static readonly List<Hook> _hooks = [];

    public static void Install()
    {
        _hooks.Add(new Hook(PanelMethod(nameof(ModOptionPanelController.Start)), OnPanelStart));
        _hooks.Add(new Hook(PanelMethod(nameof(ModOptionPanelController.LoadModOptionsFromOptionCollection)), OnModLoaded));
        _hooks.Add(new Hook(PanelMethod(nameof(ModOptionPanelController.LoadOptionListFromCategory)), OnCategoryLoaded));
    }

    private static MethodInfo PanelMethod(string name)
    {
        return typeof(ModOptionPanelController).GetMethod(name, AnyInstanceMethod);
    }

    private static void OnPanelStart(Action<ModOptionPanelController> orig, ModOptionPanelController self)
    {
        RunGuarded("generate missing options", OptionGeneration.Run);
        orig(self);
        RunGuarded("organize the mod list", () => ModListOrganizer.Attach(self));
    }

    private static void OnModLoaded(Action<ModOptionPanelController, string> orig, ModOptionPanelController self, string modGuid)
    {
        orig(self, modGuid);
        RunGuarded("update the category picker", () => CategoryPicker.ShowFor(self, modGuid));
    }

    private static void OnCategoryLoaded(Action<ModOptionPanelController, string, int> orig, ModOptionPanelController self, string modGuid, int categoryIndex)
    {
        orig(self, modGuid, categoryIndex);
        RunGuarded("fit option names", () => OptionRowFitter.AttachToRows(self));
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
