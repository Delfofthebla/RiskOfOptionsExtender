using System;
using UnityEngine;

namespace RiskOfOptionsExtender.UI;

internal static class UiObjects
{
    public static RectTransform Create(string name, Transform parent, params Type[] components)
    {
        var gameObject = new GameObject(name, [typeof(RectTransform), .. components]);
        gameObject.layer = parent.gameObject.layer;

        var rect = (RectTransform)gameObject.transform;
        rect.SetParent(parent, false);
        return rect;
    }
}
