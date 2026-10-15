using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI;

internal sealed class PanelBackground(GameObject[] layers)
{
    private static readonly string[] LayerNames = ["BlurPanel", "ImagePanel"];

    public static PanelBackground Of(Transform panelScrollView) => new(LayerNames.Select(name => panelScrollView.FindRequired(name).gameObject).ToArray());

    public void CopyInto(RectTransform target)
    {
        for (var index = 0; index < layers.Length; index++)
        {
            var layer = Object.Instantiate(layers[index], target);
            layer.name = layers[index].name;
            layer.transform.SetSiblingIndex(index);
            ((RectTransform)layer.transform).Stretch(Vector2.zero, Vector2.zero);
            layer.GetOrAddComponent<LayoutElement>().ignoreLayout = true;
        }
    }
}
