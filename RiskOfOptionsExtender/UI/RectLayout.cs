using UnityEngine;

namespace RiskOfOptionsExtender.UI;

internal readonly struct RectLayout
{
    private readonly Vector2 _anchorMin;
    private readonly Vector2 _anchorMax;
    private readonly Vector2 _pivot;
    private readonly Vector2 _offsetMin;
    private readonly Vector2 _offsetMax;

    private RectLayout(RectTransform rect)
    {
        _anchorMin = rect.anchorMin;
        _anchorMax = rect.anchorMax;
        _pivot = rect.pivot;
        _offsetMin = rect.offsetMin;
        _offsetMax = rect.offsetMax;
    }

    public static RectLayout Of(RectTransform rect)
    {
        return new RectLayout(rect);
    }

    public void ApplyTo(RectTransform rect)
    {
        rect.anchorMin = _anchorMin;
        rect.anchorMax = _anchorMax;
        rect.pivot = _pivot;
        rect.offsetMin = _offsetMin;
        rect.offsetMax = _offsetMax;
    }
}
