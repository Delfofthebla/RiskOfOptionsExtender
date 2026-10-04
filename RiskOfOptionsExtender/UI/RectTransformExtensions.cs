using UnityEngine;

namespace RiskOfOptionsExtender.UI;

internal static class RectTransformExtensions
{
    private const int BottomLeftCorner = 0;
    private const int TopRightCorner = 2;

    private static readonly Vector3[] _corners = new Vector3[4];

    public static void Stretch(this RectTransform rect, Vector2 insetMin, Vector2 insetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = insetMin;
        rect.offsetMax = -insetMax;
    }

    public static void AnchorAt(this RectTransform rect, Vector2 anchor, Vector2 pivot)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
    }

    public static Vector3 WorldBottomLeft(this RectTransform rect)
    {
        rect.GetWorldCorners(_corners);
        return _corners[BottomLeftCorner];
    }

    public static Vector3 WorldTopRight(this RectTransform rect)
    {
        rect.GetWorldCorners(_corners);
        return _corners[TopRightCorner];
    }
}
