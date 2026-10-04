using System;
using BepInEx.Configuration;
using RiskOfOptions.Components.RuntimePrefabs;
using RiskOfOptionsExtender.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.ModList;

internal sealed class ModListToolbar : MonoBehaviour
{
    private const float Height = 64f;
    private const float Gap = 8f;
    private const float SortButtonWidth = 96f;

    private ConfigEntry<ModSortOrder> _sortOrder;
    private TMP_Text _sortLabel;
    private RectTransform _scrollView;

    public static ModListToolbar Create(ModOptionsPanelPrefab panel, ConfigEntry<ModSortOrder> sortOrder, UnityAction<string> onSearchChanged)
    {
        var modListPanel = panel.ModListPanel.transform;
        var scrollView = (RectTransform)modListPanel.FindRequired("Scroll View");
        var background = PanelBackground.Of(scrollView);
        var styleSource = panel.ModListButton.GetRequiredComponentInChildren<TMP_Text>();

        var toolbarObject = new GameObject("Mod List Toolbar", typeof(RectTransform));
        try
        {
            var toolbar = toolbarObject.AddComponent<ModListToolbar>();
            toolbar.Build(panel, scrollView, background, styleSource, sortOrder, onSearchChanged);
            return toolbar;
        }
        catch
        {
            Destroy(toolbarObject);
            throw;
        }
    }

    public void Remove()
    {
        if (_scrollView)
            _scrollView.offsetMax += new Vector2(0, Height + Gap);

        Destroy(gameObject);
    }

    private void Build(ModOptionsPanelPrefab panel, RectTransform scrollView, PanelBackground background, TMP_Text styleSource,
        ConfigEntry<ModSortOrder> sortOrder, UnityAction<string> onSearchChanged)
    {
        var modListPanel = panel.ModListPanel.transform;
        gameObject.layer = modListPanel.gameObject.layer;

        var rect = (RectTransform)transform;
        rect.SetParent(modListPanel, false);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, Height);
        rect.anchoredPosition = new Vector2(0, scrollView.offsetMax.y);

        background.CopyInto(rect);

        var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        SearchField.Create(rect, styleSource, onSearchChanged);
        CreateSortButton(panel, rect);

        _sortOrder = sortOrder;
        _sortOrder.SettingChanged += OnSortOrderChanged;
        RefreshSortLabel();

        _scrollView = scrollView;
        _scrollView.offsetMax -= new Vector2(0, Height + Gap);
    }

    private void CreateSortButton(ModOptionsPanelPrefab panel, RectTransform toolbar)
    {
        var buttonObject = UiButtons.Clone(panel.CategoryLeftButton, toolbar, "Sort Order Button", CycleSortOrder);

        var layoutElement = buttonObject.GetOrAddComponent<LayoutElement>();
        layoutElement.minWidth = SortButtonWidth;
        layoutElement.preferredWidth = SortButtonWidth;

        _sortLabel = buttonObject.GetRequiredComponentInChildren<TMP_Text>();
        _sortLabel.fontSize = 24;
        _sortLabel.alignment = TextAlignmentOptions.Center;
    }

    private void OnDestroy()
    {
        if (_sortOrder != null)
            _sortOrder.SettingChanged -= OnSortOrderChanged;
    }

    private void OnSortOrderChanged(object sender, EventArgs args)
    {
        RefreshSortLabel();
    }

    private void CycleSortOrder()
    {
        _sortOrder.Value = _sortOrder.Value == ModSortOrder.Alphabetical
            ? ModSortOrder.ReverseAlphabetical
            : ModSortOrder.Alphabetical;
    }

    private void RefreshSortLabel()
    {
        if (_sortLabel)
            _sortLabel.text = _sortOrder.Value == ModSortOrder.Alphabetical ? "A-Z" : "Z-A";
    }
}
