using UnityEngine;

namespace RiskOfOptionsExtender.UI;

internal readonly record struct RectLayout(Vector2 AnchorMin, Vector2 AnchorMax, Vector2 Pivot, Vector2 OffsetMin, Vector2 OffsetMax)
{
    public static RectLayout Of(RectTransform rect) => new(rect.anchorMin, rect.anchorMax, rect.pivot, rect.offsetMin, rect.offsetMax);

    public void ApplyTo(RectTransform rect)
    {
        rect.anchorMin = AnchorMin;
        rect.anchorMax = AnchorMax;
        rect.pivot = Pivot;
        rect.offsetMin = OffsetMin;
        rect.offsetMax = OffsetMax;
    }
}
