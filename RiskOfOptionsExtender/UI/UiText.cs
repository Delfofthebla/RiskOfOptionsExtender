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
        ((RectTransform)textObject.transform).Stretch(Vector2.zero, Vector2.zero);

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
}
