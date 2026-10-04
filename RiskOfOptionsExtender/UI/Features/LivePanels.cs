using System.Collections.Generic;
using RiskOfOptions.Components.Panel;
using RoR2.UI;

namespace RiskOfOptionsExtender.UI.Features;

internal static class LivePanels
{
    private static readonly List<ModOptionPanelController> _panels = [];

    public static IReadOnlyList<ModOptionPanelController> All
    {
        get
        {
            _panels.RemoveAll(panel => !panel);
            return _panels.ToArray();
        }
    }

    public static void Register(ModOptionPanelController panel)
    {
        if (!_panels.Contains(panel))
            _panels.Add(panel);
    }

    public static string OpenModGuid(ModOptionPanelController panel)
    {
        var prefab = panel._panel;
        if (prefab == null || !prefab.CategoryHeader || !prefab.CategoryHeader.activeSelf)
            return null;

        var navigation = prefab.ModListPanel.GetRequiredComponent<HGHeaderNavigationController>();
        return (navigation.SelectedButton() as ModListButton)?.modGuid;
    }
}
