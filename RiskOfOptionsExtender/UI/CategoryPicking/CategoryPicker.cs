using System.Collections.Generic;
using System.Linq;
using RiskOfOptions;
using RiskOfOptions.Components.Panel;
using RiskOfOptions.Components.RuntimePrefabs;
using RiskOfOptions.Containers;
using RiskOfOptionsExtender.Resilience;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.CategoryPicking;

internal sealed class CategoryPicker : MonoBehaviour
{
    private const int TabsPerPage = 4;
    private const int PickerButtonSlots = 1;
    private const float MaxTabWidth = 200f;
    private const float Margin = 12f;
    private const float ArrowSize = 64f;
    private const float RightArrowCenterFromEdge = 60f;
    private const float RightArrowAreaWidth = RightArrowCenterFromEdge + ArrowSize / 2 + Margin;
    private const float LeftOfTabsFixedWidth = Margin + Margin + ArrowSize + Margin;
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
    private FeatureGuard _guard;
    private OptionCollection _collection;
    private bool _layoutPending;
    private RectLayout _viewportLayout;
    private RectLayout _leftArrowLayout;

    public static void ShowFor(ModOptionPanelController panelController, string modGuid, FeatureGuard guard)
    {
        var picker = GetOrCreate(panelController._panel, guard);
        var collection = ModSettingsManager.OptionCollection[modGuid];

        if (collection.CategoryCount > TabsPerPage)
            picker.Show(collection);
        else
            picker.Hide();
    }

    public static void RemoveAll()
    {
        foreach (var picker in _livePickers.ToList())
            picker.Remove();
    }

    private static CategoryPicker GetOrCreate(ModOptionsPanelPrefab panel, FeatureGuard guard)
    {
        var picker = panel.CategoryHeader.GetComponent<CategoryPicker>();
        if (picker)
            return picker;

        picker = panel.CategoryHeader.AddComponent<CategoryPicker>();
        try
        {
            picker.Initialize(panel, guard);
        }
        catch
        {
            picker.DestroyCreatedObjects();
            throw;
        }

        _livePickers.Add(picker);
        return picker;
    }

    private void Initialize(ModOptionsPanelPrefab panel, FeatureGuard guard)
    {
        _guard = guard;
        _scrollView = (RectTransform)panel.CategoryHeader.transform.FindRequired("Scroll View");
        _viewport = (RectTransform)_scrollView.FindRequired("Viewport");
        _leftArrow = (RectTransform)panel.CategoryLeftButton.transform;
        _tabs = _scrollView.GetRequiredComponent<CategoryScrollRect>();
        _tabNavigation = _tabs.categoryTransform.GetRequiredComponent<HGHeaderNavigationController>();
        _viewportLayout = RectLayout.Of(_viewport);
        _leftArrowLayout = RectLayout.Of(_leftArrow);

        CreateButton(panel);

        var modListScrollView = panel.ModListPanel.transform.FindRequired("Scroll View");
        _popup = CategoryPickerPopup.Create(panel.CategoryHeader.transform, _button, _buttonLabel, modListScrollView, SelectCategory);
    }

    private void CreateButton(ModOptionsPanelPrefab panel)
    {
        var buttonObject = UiButtons.Clone(panel.CategoryHeaderButton, _scrollView, "Category Picker Button", TogglePopup);
        buttonObject.SetActive(false);

        _button = (RectTransform)buttonObject.transform;
        _button.AnchorAt(new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        _button.anchoredPosition = new Vector2(Margin, 0);
        _button.sizeDelta = new Vector2(MaxTabWidth, _viewport.rect.height * TabHeightShareOfViewport);

        _buttonLabel = buttonObject.GetRequiredComponentInChildren<TMP_Text>();
    }

    private void Show(OptionCollection collection)
    {
        _collection = collection;
        _popup.Close();

        _buttonLabel.text = $"All Tabs ({collection.CategoryCount})";
        _button.gameObject.SetActive(true);
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
        DestroyCreatedObjects();
    }

    private void DestroyCreatedObjects()
    {
        if (_popup)
            Destroy(_popup.gameObject);

        if (_button)
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
            _guard.Run("lay out the category tabs", () => _layoutPending = !TryFitPageOfTabs());
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

        var widthForTabs = scrollViewWidth - LeftOfTabsFixedWidth - RightArrowAreaWidth - gaps;
        var tabWidth = Mathf.Min(MaxTabWidth, widthForTabs / (TabsPerPage + PickerButtonSlots));
        var leftArrowLeft = Margin + tabWidth + Margin;
        var viewportLeft = leftArrowLeft + ArrowSize + Margin;

        PlaceLeftArrowAt(leftArrowLeft);

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

    private void PlaceLeftArrowAt(float left)
    {
        _leftArrow.anchorMin = new Vector2(0, _leftArrow.anchorMin.y);
        _leftArrow.anchorMax = new Vector2(0, _leftArrow.anchorMax.y);
        _leftArrow.anchoredPosition = new Vector2(left + _leftArrow.pivot.x * ArrowSize, _leftArrow.anchoredPosition.y);
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
            _guard.Run("open the All Tabs picker", () => _popup.Open(PickerChoices.For(_collection), Mathf.Max(0, _tabNavigation.currentHeaderIndex)));
    }

    private void SelectCategory(int categoryIndex)
    {
        _popup.Close();

        if (categoryIndex < _tabs.categoryButtons.Count)
            _guard.Run("switch tabs from the All Tabs picker", () => SwitchTo(categoryIndex));
    }

    private void SwitchTo(int categoryIndex)
    {
        _tabs.categoryButtons[categoryIndex].GetRequiredComponentInChildren<HGButton>().onClick.Invoke();
        _tabs.SetPage(categoryIndex / TabsPerPage);
    }
}
