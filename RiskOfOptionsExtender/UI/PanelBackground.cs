using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI;

internal sealed class PanelBackground
{
    private static readonly string[] LayerNames = ["BlurPanel", "ImagePanel"];

    private readonly GameObject[] _layers;

    private PanelBackground(GameObject[] layers)
    {
        _layers = layers;
    }

    public static PanelBackground Of(Transform panelScrollView)
    {
        return new PanelBackground(LayerNames.Select(name => panelScrollView.FindRequired(name).gameObject).ToArray());
    }

    public void CopyInto(RectTransform target)
    {
        for (var index = 0; index < _layers.Length; index++)
        {
            var layer = Object.Instantiate(_layers[index], target);
            layer.name = _layers[index].name;
            layer.transform.SetSiblingIndex(index);
            ((RectTransform)layer.transform).Stretch(Vector2.zero, Vector2.zero);
            layer.GetOrAddComponent<LayoutElement>().ignoreLayout = true;
        }
    }
}
