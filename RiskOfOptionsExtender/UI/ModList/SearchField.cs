using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.ModList;

internal static class SearchField
{
    private static readonly float FontSize = 22f;
    private static readonly float MinWidth = 64f;
    private static readonly Vector2 TextInset = new(12f, 4f);

    private static readonly Color Background = new(0f, 0f, 0f, 0.45f);
    private static readonly Color PlaceholderColor = new(1f, 1f, 1f, 0.4f);

    public static void Create(Transform parent, TMP_Text styleSource, UnityAction<string> onChanged)
    {
        var root = UiObjects.Create("Search Field", parent, typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement)).gameObject;
        root.SetActive(false);

        root.GetComponent<Image>().color = Background;
        var layoutElement = root.GetComponent<LayoutElement>();
        layoutElement.flexibleWidth = 1;
        layoutElement.minWidth = MinWidth;

        var viewport = UiObjects.Create("Text Area", root.transform, typeof(RectMask2D));
        viewport.Stretch(TextInset, TextInset);

        var text = UiText.Create("Text", viewport, styleSource, FontSize, Color.white);
        var placeholder = UiText.Create("Placeholder", viewport, styleSource, FontSize, PlaceholderColor);
        placeholder.text = "Search mods...";
        placeholder.fontStyle = FontStyles.Italic;

        var input = root.AddComponent<TMP_InputField>();
        input.textViewport = viewport;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.fontAsset = text.font;
        input.pointSize = FontSize;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.richText = false;
        input.navigation = new Navigation { mode = Navigation.Mode.None };
        input.onValueChanged.AddListener(onChanged);

        root.SetActive(true);
    }
}
