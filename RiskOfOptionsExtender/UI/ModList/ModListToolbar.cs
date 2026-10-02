using System;
using RiskOfOptions.Components.RuntimePrefabs;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.ModList;

internal sealed class ModListToolbar : MonoBehaviour
{
    private const float Height = 64f;
    private const float Gap = 8f;
    private const float SortButtonWidth = 96f;

    private TMP_Text _sortLabel;

    private static ExtenderSettings Settings => RiskOfOptionsExtenderPlugin.Settings;

    public static void Create(ModOptionsPanelPrefab panel, ModListOrganizer organizer)
    {
        var modListPanel = panel.ModListPanel.transform;
        var scrollView = (RectTransform)modListPanel.Find("Scroll View");
        scrollView.offsetMax -= new Vector2(0, Height + Gap);

        var toolbarObject = new GameObject("Mod List Toolbar", typeof(RectTransform));
        toolbarObject.layer = modListPanel.gameObject.layer;

        var rect = (RectTransform)toolbarObject.transform;
        rect.SetParent(modListPanel, false);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, Height);
        rect.anchoredPosition = new Vector2(0, scrollView.offsetMax.y + Height + Gap);

        PanelBackground.CopyInto(rect, scrollView);

        var layout = toolbarObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        var styleSource = panel.ModListButton.GetComponentInChildren<TMP_Text>(true);
        SearchField.Create(rect, styleSource, organizer.SetFilter);

        var toolbar = toolbarObject.AddComponent<ModListToolbar>();
        toolbar.CreateSortButton(panel, rect);
    }

    private static LayoutElement LayoutElementOf(GameObject gameObject)
    {
        var layoutElement = gameObject.GetComponent<LayoutElement>();
        return layoutElement ? layoutElement : gameObject.AddComponent<LayoutElement>();
    }

    private void CreateSortButton(ModOptionsPanelPrefab panel, RectTransform toolbar)
    {
        var buttonObject = Instantiate(panel.CategoryLeftButton, toolbar);
        buttonObject.name = "Sort Order Button";
        buttonObject.SetActive(true);

        var layoutElement = LayoutElementOf(buttonObject);
        layoutElement.minWidth = SortButtonWidth;
        layoutElement.preferredWidth = SortButtonWidth;

        foreach (var languageController in buttonObject.GetComponentsInChildren<LanguageTextMeshController>(true))
            DestroyImmediate(languageController);

        _sortLabel = buttonObject.GetComponentInChildren<TMP_Text>(true);
        _sortLabel.fontSize = 24;
        _sortLabel.alignment = TextAlignmentOptions.Center;

        var button = buttonObject.GetComponent<HGButton>();
        button.disablePointerClick = false;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(CycleSortOrder);

        RefreshSortLabel();
    }

    private void OnEnable()
    {
        Settings.SortOrder.SettingChanged += OnSortOrderChanged;
    }

    private void OnDisable()
    {
        Settings.SortOrder.SettingChanged -= OnSortOrderChanged;
    }

    private void OnSortOrderChanged(object sender, EventArgs args)
    {
        RefreshSortLabel();
    }

    private void CycleSortOrder()
    {
        Settings.SortOrder.Value = Settings.SortOrder.Value == ModSortOrder.Alphabetical
            ? ModSortOrder.ReverseAlphabetical
            : ModSortOrder.Alphabetical;
    }

    private void RefreshSortLabel()
    {
        if (_sortLabel)
            _sortLabel.text = Settings.SortOrder.Value == ModSortOrder.Alphabetical ? "A-Z" : "Z-A";
    }
}
