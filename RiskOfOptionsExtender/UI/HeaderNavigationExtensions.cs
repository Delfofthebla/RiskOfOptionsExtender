using RoR2.UI;

namespace RiskOfOptionsExtender.UI;

internal static class HeaderNavigationExtensions
{
    public static MPButton SelectedButton(this HGHeaderNavigationController navigation)
    {
        var index = navigation.currentHeaderIndex;
        var headers = navigation.headers;
        return index >= 0 && index < headers.Count ? headers[index].headerButton : null;
    }
}
