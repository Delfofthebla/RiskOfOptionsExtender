using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.ModList;

internal static class SearchField
{
    private const float FontSize = 22f;

    private static readonly Color Background = new(0f, 0f, 0f, 0.45f);
    private static readonly Color PlaceholderColor = new(1f, 1f, 1f, 0.4f);

    public static TMP_InputField Create(Transform parent, TMP_Text styleSource, UnityAction<string> onChanged)
    {
        var root = new GameObject("Search Field", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        root.SetActive(false);
        root.layer = parent.gameObject.layer;
        root.transform.SetParent(parent, false);

        root.GetComponent<Image>().color = Background;
        var layoutElement = root.GetComponent<LayoutElement>();
        layoutElement.flexibleWidth = 1;
        layoutElement.minWidth = 64;

        var viewport = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        viewport.layer = root.layer;
        viewport.transform.SetParent(root.transform, false);
        var viewportRect = (RectTransform)viewport.transform;
        viewportRect.Stretch(new Vector2(12, 4), new Vector2(12, 4));

        var text = UiText.Create("Text", viewportRect, styleSource, FontSize, Color.white);
        var placeholder = UiText.Create("Placeholder", viewportRect, styleSource, FontSize, PlaceholderColor);
        placeholder.text = "Search mods...";
        placeholder.fontStyle = FontStyles.Italic;

        var input = root.AddComponent<TMP_InputField>();
        input.textViewport = viewportRect;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.fontAsset = text.font;
        input.pointSize = FontSize;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.richText = false;
        input.navigation = new Navigation { mode = Navigation.Mode.None };
        input.onValueChanged.AddListener(onChanged);

        root.SetActive(true);
        return input;
    }
}
