using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.CategoryPicking;

internal sealed class PickerItem : PointerTarget
{
    private const float CategoryHeight = 40f;
    private const float HeaderHeight = 32f;
    private const float CategoryFontSize = 22f;
    private const float HeaderFontSize = 18f;
    private const float CategoryIndent = 24f;
    private const float HeaderIndent = 10f;

    private static readonly Color IdleColor = new(1f, 1f, 1f, 0f);
    private static readonly Color HoveredColor = new(1f, 1f, 1f, 0.1f);
    private static readonly Color CurrentColor = new(1f, 1f, 1f, 0.2f);
    private static readonly Color HeaderTextColor = new(0.6f, 0.6f, 0.6f, 1f);

    private Image _background;
    private Action<int> _onSelected;
    private bool _current;

    public int CategoryIndex { get; private set; }

    public static float HeightOf(PickerChoice choice)
    {
        return choice.IsHeader ? HeaderHeight : CategoryHeight;
    }

    public static void CreateHeader(Transform parent, TMP_Text styleSource, string text)
    {
        var row = CreateRow("Group Header", parent, HeaderHeight);
        var label = UiText.Create("Text", row.transform, styleSource, HeaderFontSize, HeaderTextColor);
        label.rectTransform.offsetMin = new Vector2(HeaderIndent, 0);
        label.text = text;
    }

    public static PickerItem CreateCategory(Transform parent, TMP_Text styleSource, string text, int categoryIndex, Action<int> onSelected)
    {
        var row = CreateRow("Category", parent, CategoryHeight);
        var label = UiText.Create("Text", row.transform, styleSource, CategoryFontSize, Color.white);
        label.rectTransform.offsetMin = new Vector2(CategoryIndent, 0);
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.text = text;

        var item = row.AddComponent<PickerItem>();
        item._background = row.GetComponent<Image>();
        item._onSelected = onSelected;
        item.CategoryIndex = categoryIndex;
        item.Refresh();
        return item;
    }

    public void SetCurrent(bool current)
    {
        _current = current;
        Refresh();
    }

    protected override void OnLeftClick()
    {
        _onSelected(CategoryIndex);
    }

    protected override void Refresh()
    {
        if (_current)
            _background.color = CurrentColor;
        else
            _background.color = IsHovered ? HoveredColor : IdleColor;
    }

    private static GameObject CreateRow(string name, Transform parent, float height)
    {
        var row = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        row.layer = parent.gameObject.layer;
        row.transform.SetParent(parent, false);
        row.GetComponent<Image>().color = IdleColor;

        var layoutElement = row.GetComponent<LayoutElement>();
        layoutElement.minHeight = height;
        layoutElement.preferredHeight = height;
        return row;
    }
}
