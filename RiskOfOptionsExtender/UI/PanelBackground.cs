using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI;

internal static class PanelBackground
{
    private static readonly string[] LayerNames = ["BlurPanel", "ImagePanel"];

    public static void CopyInto(RectTransform target, Transform panelScrollView)
    {
        for (var index = 0; index < LayerNames.Length; index++)
        {
            var layer = Object.Instantiate(panelScrollView.Find(LayerNames[index]).gameObject, target);
            layer.name = LayerNames[index];
            layer.transform.SetSiblingIndex(index);
            UiText.Stretch((RectTransform)layer.transform, Vector2.zero, Vector2.zero);

            var layoutElement = layer.GetComponent<LayoutElement>();
            if (!layoutElement)
                layoutElement = layer.AddComponent<LayoutElement>();

            layoutElement.ignoreLayout = true;
        }
    }
}
