using System.Collections.Generic;
using System.Linq;
using RiskOfOptions;
using RiskOfOptions.Components.Panel;
using RiskOfOptions.Components.RuntimePrefabs;
using RiskOfOptions.Containers;
using RiskOfOptionsExtender.Generation;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.CategoryPicking;

internal sealed class CategoryPicker : MonoBehaviour
{
    private const int TabsPerPage = 4;
    private const float MaxTabWidth = 200f;
    private const float Margin = 12f;
    private const float ArrowSize = 64f;
    private const float ArrowSpacing = 8f;
    private const float ArrowCenterFromEdge = 60f;
    private const float ArrowAreaWidth = ArrowCenterFromEdge + ArrowSize + ArrowSpacing + ArrowSize / 2 + Margin;
    private const float TabHeightShareOfViewport = 0.6f;

    private static readonly HashSet<CategoryPicker> _livePickers = [];

    private RectTransform _scrollView;
    private RectTransform _viewport;
    private RectTransform _leftArrow;
    private CategoryScrollRect _tabs;
    private HGHeaderNavigationController _tabNavigation;
    private RectTransform _button;
    private TMP_Text _buttonLabel;
    private CategoryPickerPopup _popup;
    private OptionCollection _collection;
    private int _currentCategory;
    private bool _layoutPending;
    private RectLayout _viewportLayout;
    private RectLayout _leftArrowLayout;

    public static void ShowFor(ModOptionPanelController panelController, string modGuid)
    {
        var picker = GetOrCreate(panelController._panel);
        var collection = ModSettingsManager.OptionCollection[modGuid];

        if (collection.CategoryCount > TabsPerPage)
            picker.Show(collection);
        else
            picker.Hide();
    }

    public static void SyncSelection(ModOptionPanelController panelController, int categoryIndex)
    {
        var picker = panelController._panel.CategoryHeader.GetComponent<CategoryPicker>();
        if (!picker)
            return;

        picker._currentCategory = categoryIndex;
        picker._popup.MarkCurrent(categoryIndex);
    }

    public static void RemoveAll()
    {
        foreach (var picker in _livePickers.ToList())
            picker.Remove();
    }

    private static CategoryPicker GetOrCreate(ModOptionsPanelPrefab panel)
    {
        var picker = panel.CategoryHeader.GetComponent<CategoryPicker>();
        if (picker)
            return picker;

        picker = panel.CategoryHeader.AddComponent<CategoryPicker>();
        picker.Initialize(panel);
        _livePickers.Add(picker);
        return picker;
    }

    private void Initialize(ModOptionsPanelPrefab panel)
    {
        var scrollView = (RectTransform)panel.CategoryHeader.transform.Find("Scroll View");
        _scrollView = scrollView;
        _viewport = (RectTransform)scrollView.Find("Viewport");
        _leftArrow = (RectTransform)panel.CategoryLeftButton.transform;
        _tabs = scrollView.GetComponent<CategoryScrollRect>();
        _tabNavigation = _tabs.categoryTransform.GetComponent<HGHeaderNavigationController>();
        _viewportLayout = RectLayout.Of(_viewport);
        _leftArrowLayout = RectLayout.Of(_leftArrow);

        CreateButton(panel, scrollView);

        var modListScrollView = panel.ModListPanel.transform.Find("Scroll View");
        _popup = CategoryPickerPopup.Create(panel.CategoryHeader.transform, _button, _buttonLabel, modListScrollView, SelectCategory);
    }

    private void CreateButton(ModOptionsPanelPrefab panel, RectTransform scrollView)
    {
        var buttonObject = Instantiate(panel.CategoryHeaderButton, scrollView);
        buttonObject.name = "Category Picker Button";
        buttonObject.SetActive(true);

        foreach (var languageController in buttonObject.GetComponentsInChildren<LanguageTextMeshController>(true))
            DestroyImmediate(languageController);

        _button = (RectTransform)buttonObject.transform;
        _button.anchorMin = new Vector2(0, 0.5f);
        _button.anchorMax = new Vector2(0, 0.5f);
        _button.pivot = new Vector2(0, 0.5f);
        _button.anchoredPosition = new Vector2(Margin, 0);
        _button.sizeDelta = new Vector2(MaxTabWidth, _viewport.rect.height * TabHeightShareOfViewport);

        _buttonLabel = buttonObject.GetComponentInChildren<TMP_Text>(true);

        var hgButton = buttonObject.GetComponentInChildren<HGButton>();
        hgButton.disablePointerClick = false;
        hgButton.onClick.RemoveAllListeners();
        hgButton.onClick.AddListener(TogglePopup);

        buttonObject.SetActive(false);
    }

    private void Show(OptionCollection collection)
    {
        _collection = collection;
        _currentCategory = Mathf.Max(0, _tabNavigation.currentHeaderIndex);
        _popup.Close();

        _buttonLabel.text = $"All Tabs ({collection.CategoryCount})";
        _button.gameObject.SetActive(true);

        _leftArrow.anchorMin = new Vector2(1, 0.5f);
        _leftArrow.anchorMax = new Vector2(1, 0.5f);
        _leftArrow.anchoredPosition = new Vector2(-(ArrowCenterFromEdge + ArrowSize + ArrowSpacing), 0);

        _layoutPending = !TryFitPageOfTabs();
    }

    private void Hide()
    {
        _collection = null;
        _layoutPending = false;
        _popup.Close();
        _button.gameObject.SetActive(false);
        _viewportLayout.ApplyTo(_viewport);
        _leftArrowLayout.ApplyTo(_leftArrow);

        foreach (var tab in _tabs.categoryButtons)
            SetPreferredWidth(tab, MaxTabWidth);
    }

    private void Remove()
    {
        Hide();
        Destroy(_popup.gameObject);
        Destroy(_button.gameObject);
        Destroy(this);
    }

    private void OnDestroy()
    {
        _livePickers.Remove(this);
    }

    private void LateUpdate()
    {
        if (_layoutPending)
            _layoutPending = !TryFitPageOfTabs();
    }

    // The tab strip has its own canvas, which escapes the viewport's clipping, so the viewport is sized to exactly one
    // page of tabs instead of relying on the clip to hide the overflow.
    private bool TryFitPageOfTabs()
    {
        var scrollViewWidth = _scrollView.rect.width;
        if (scrollViewWidth < 1 || _tabs.categoryButtons.Count == 0)
            return false;

        var tabStrip = (RectTransform)_tabs.categoryTransform;
        var stripLayout = tabStrip.GetComponent<HorizontalLayoutGroup>();
        var gaps = stripLayout ? (TabsPerPage - 1) * stripLayout.spacing + stripLayout.padding.horizontal : 0f;

        var tabWidth = Mathf.Min(MaxTabWidth, (scrollViewWidth - 2 * Margin - ArrowAreaWidth - gaps) / (TabsPerPage + 1));
        var viewportLeft = 2 * Margin + tabWidth;

        _viewport.anchorMin = new Vector2(0, _viewport.anchorMin.y);
        _viewport.anchorMax = new Vector2(0, _viewport.anchorMax.y);
        _viewport.offsetMin = new Vector2(viewportLeft, _viewport.offsetMin.y);
        _viewport.offsetMax = new Vector2(viewportLeft + TabsPerPage * tabWidth + gaps, _viewport.offsetMax.y);

        foreach (var tab in _tabs.categoryButtons)
            SetPreferredWidth(tab, tabWidth);

        LayoutRebuilder.ForceRebuildLayoutImmediate(tabStrip);
        _tabs.SetPage(_tabs._currentPage, true);

        _button.sizeDelta = new Vector2(tabWidth, TabHeight());
        return true;
    }

    private float TabHeight()
    {
        var measured = ((RectTransform)_tabs.categoryButtons[0].transform).rect.height;
        return measured >= 1 ? measured : _viewport.rect.height * TabHeightShareOfViewport;
    }

    private static void SetPreferredWidth(GameObject tab, float width)
    {
        if (!tab)
            return;

        var layoutElement = tab.GetComponent<LayoutElement>();
        if (layoutElement)
            layoutElement.preferredWidth = width;
    }

    private void OnDisable()
    {
        if (_popup)
            _popup.Close();
    }

    private void TogglePopup()
    {
        if (_popup.IsOpen)
            _popup.Close();
        else if (_collection != null)
            _popup.Open(BuildChoices(_collection), _currentCategory);
    }

    private void SelectCategory(int categoryIndex)
    {
        _popup.Close();

        var tabButton = _tabs.categoryButtons[categoryIndex].GetComponentInChildren<HGButton>();
        tabButton.onClick.Invoke();
        _tabs.SetPage(categoryIndex / TabsPerPage);
    }

    private static List<PickerChoice> BuildChoices(OptionCollection collection)
    {
        var groups = Enumerable.Range(0, collection.CategoryCount)
            .Select(index => CategoryGroups.FileLabelOf(collection.ModGuid, collection[index].name) ?? collection.ModName)
            .ToList();
        var isGrouped = groups.Distinct().Count() > 1;

        var choices = new List<PickerChoice>();
        string currentGroup = null;
        for (var index = 0; index < collection.CategoryCount; index++)
        {
            if (isGrouped && groups[index] != currentGroup)
            {
                currentGroup = groups[index];
                choices.Add(PickerChoice.Header(currentGroup));
            }

            choices.Add(PickerChoice.Category(Language.GetString(collection[index].NameToken), index));
        }

        return choices;
    }
}
