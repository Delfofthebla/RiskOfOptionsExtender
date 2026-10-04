using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.CategoryPicking;

internal sealed class CategoryPickerPopup : MonoBehaviour
{
    private const float Width = 360f;
    private const float MaxHeight = 520f;
    private const float Padding = 6f;
    private const float Spacing = 2f;
    private const float ScrollbarWidth = 24f;
    private const float GapBelowButton = 4f;
    private const int SortingOrder = 30000;

    private readonly List<PickerItem> _items = [];
    private RectTransform _rect;
    private RectTransform _content;
    private ScrollRect _scroll;
    private Canvas _canvas;
    private PopupBlocker _blocker;
    private RectTransform _anchorButton;
    private TMP_Text _styleSource;
    private Action<int> _onSelected;

    public bool IsOpen => gameObject.activeSelf;

    public static CategoryPickerPopup Create(Transform parent, RectTransform anchorButton, TMP_Text styleSource, Transform panelScrollView, Action<int> onSelected)
    {
        var background = PanelBackground.Of(panelScrollView);
        var scrollbarTemplate = panelScrollView.FindRequired("Scrollbar Vertical").gameObject;

        var root = new GameObject("Category Picker Popup", typeof(RectTransform));
        root.SetActive(false);
        root.layer = parent.gameObject.layer;
        root.transform.SetParent(parent, false);

        var popup = root.AddComponent<CategoryPickerPopup>();
        popup._anchorButton = anchorButton;
        popup._styleSource = styleSource;
        popup._onSelected = onSelected;
        popup.Build(background, scrollbarTemplate);
        return popup;
    }

    public void Open(IReadOnlyList<PickerChoice> choices, int currentCategory)
    {
        RebuildItems(choices);
        gameObject.SetActive(true);
        DrawAboveSiblingCanvases();

        _rect.sizeDelta = new Vector2(Width, Mathf.Min(ContentHeight(choices), MaxHeight));
        PlaceBelowButton();
        MarkCurrent(currentCategory);
        ScrollToCurrent(choices, currentCategory);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void MarkCurrent(int categoryIndex)
    {
        foreach (var item in _items)
            item.SetCurrent(item.CategoryIndex == categoryIndex);
    }

    // Unity discards a nested canvas's sorting override while its object is inactive, so it is reapplied on every open.
    private void DrawAboveSiblingCanvases()
    {
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = SortingOrder;
        _blocker.DrawBelow(SortingOrder);
    }

    private void Build(PanelBackground background, GameObject scrollbarTemplate)
    {
        _rect = (RectTransform)transform;
        _rect.AnchorAt(new Vector2(0.5f, 0.5f), new Vector2(0, 1));

        _canvas = gameObject.AddComponent<Canvas>();
        gameObject.AddComponent<GraphicRaycaster>();
        gameObject.AddComponent<Image>().color = Color.clear;
        background.CopyInto(_rect);
        _blocker = PopupBlocker.Create(_rect, Close);

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.layer = gameObject.layer;
        var viewportRect = (RectTransform)viewport.transform;
        viewportRect.SetParent(_rect, false);
        viewportRect.Stretch(new Vector2(Padding, Padding), new Vector2(Padding + ScrollbarWidth, Padding));

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.layer = gameObject.layer;
        _content = (RectTransform)content.transform;
        _content.SetParent(viewportRect, false);
        _content.anchorMin = new Vector2(0, 1);
        _content.anchorMax = Vector2.one;
        _content.pivot = new Vector2(0.5f, 1);
        _content.sizeDelta = Vector2.zero;

        var layout = content.GetComponent<VerticalLayoutGroup>();
        layout.spacing = Spacing;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _scroll = gameObject.AddComponent<ScrollRect>();
        _scroll.viewport = viewportRect;
        _scroll.content = _content;
        _scroll.horizontal = false;
        _scroll.movementType = ScrollRect.MovementType.Clamped;
        _scroll.inertia = false;
        _scroll.scrollSensitivity = 40f;

        AddScrollbar(scrollbarTemplate);
    }

    private void AddScrollbar(GameObject scrollbarTemplate)
    {
        var scrollbarObject = Instantiate(scrollbarTemplate, _rect);
        scrollbarObject.name = "Scrollbar";

        var scrollbarRect = (RectTransform)scrollbarObject.transform;
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = Vector2.one;
        scrollbarRect.pivot = new Vector2(1, 0.5f);
        scrollbarRect.offsetMin = new Vector2(-(ScrollbarWidth + Padding / 2), Padding);
        scrollbarRect.offsetMax = new Vector2(-Padding / 2, -Padding);

        _scroll.verticalScrollbar = scrollbarObject.GetComponent<Scrollbar>();
        _scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
    }

    private void RebuildItems(IReadOnlyList<PickerChoice> choices)
    {
        _items.Clear();
        for (var index = _content.childCount - 1; index >= 0; index--)
            DestroyImmediate(_content.GetChild(index).gameObject);

        foreach (var choice in choices)
        {
            if (choice.IsHeader)
                PickerItem.CreateHeader(_content, _styleSource, choice.Text);
            else
                _items.Add(PickerItem.CreateCategory(_content, _styleSource, choice.Text, choice.CategoryIndex, _onSelected));
        }
    }

    private void PlaceBelowButton()
    {
        _rect.position = _anchorButton.WorldBottomLeft();
        _rect.anchoredPosition -= new Vector2(0, GapBelowButton);
    }

    private void ScrollToCurrent(IReadOnlyList<PickerChoice> choices, int currentCategory)
    {
        var offset = 0f;
        foreach (var choice in choices)
        {
            if (!choice.IsHeader && choice.CategoryIndex == currentCategory)
                break;

            offset += PickerItem.HeightOf(choice) + Spacing;
        }

        Canvas.ForceUpdateCanvases();
        var scrollable = _content.rect.height - ((RectTransform)_scroll.viewport).rect.height;
        _scroll.verticalNormalizedPosition = scrollable <= 0 ? 1 : 1 - Mathf.Clamp01(offset / scrollable);
    }

    private static float ContentHeight(IReadOnlyList<PickerChoice> choices)
    {
        var height = Padding * 2 + Spacing * Math.Max(0, choices.Count - 1);
        foreach (var choice in choices)
            height += PickerItem.HeightOf(choice);

        return height;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }
}
