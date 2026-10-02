using RoR2.UI;
using TMPro;
using UnityEngine;

namespace RiskOfOptionsExtender.UI;

internal static class UiText
{
    public static HGTextMeshProUGUI Create(string name, Transform parent, TMP_Text styleSource, float fontSize, Color color)
    {
        var textObject = new GameObject(name, typeof(RectTransform));
        textObject.layer = parent.gameObject.layer;
        textObject.transform.SetParent(parent, false);
        Stretch((RectTransform)textObject.transform, Vector2.zero, Vector2.zero);

        var text = textObject.AddComponent<HGTextMeshProUGUI>();
        text.font = styleSource.font;
        text.fontSharedMaterial = styleSource.fontSharedMaterial;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        return text;
    }

    public static void Stretch(RectTransform rect, Vector2 insetMin, Vector2 insetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = insetMin;
        rect.offsetMax = -insetMax;
    }
}
