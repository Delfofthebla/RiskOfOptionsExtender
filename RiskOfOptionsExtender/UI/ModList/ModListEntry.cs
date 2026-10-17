using RiskOfOptions.Components.Panel;
using RoR2.UI;

namespace RiskOfOptionsExtender.UI.ModList;

internal sealed record ModListEntry(ModListButton Button, HGHeaderNavigationController.Header Header, string DisplayName, bool IsEmpty)
{
    public string ModGuid => Button.modGuid;
}
