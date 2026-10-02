using RiskOfOptions.Components.Panel;
using RoR2.UI;

namespace RiskOfOptionsExtender.UI.ModList;

internal sealed class ModListEntry(ModListButton button, HGHeaderNavigationController.Header header, string displayName, bool isEmpty)
{
    public ModListButton Button { get; } = button;

    public HGHeaderNavigationController.Header Header { get; } = header;

    public string DisplayName { get; } = displayName;

    public bool IsEmpty { get; } = isEmpty;

    public string ModGuid => Button.modGuid;
}
