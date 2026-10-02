using System.Collections.Generic;
using RiskOfOptions;
using RiskOfOptions.Components.Options;
using RiskOfOptions.Components.Panel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RooAssets = RiskOfOptions.Resources.Assets;

namespace RiskOfOptionsExtender.UI.OptionRows;

internal sealed class OptionRowFitter : MonoBehaviour
{
    private const float GapBeforeControl = 12f;
    private const float CheckboxRightInset = 12f;
    private const float IconSize = 26f;
    private const float IconGap = 6f;
    private const string RestartIconPath = "assets/RiskOfOptions/ror2RestartSymbol.png";

    private static readonly Color IconColor = new(1f, 0.5f, 0.5f, 0.85f);
    private static readonly HashSet<string> DecorationNames = ["BaseOutline", "HoverOutline", "Mod Settings Indicator"];

    private RectTransform _row;
    private RectTransform _label;
    private TMP_Text _text;
    private Image _checkbox;
    private bool _restartRequired;

    public static void AttachToRows(ModOptionPanelController panelController)
    {
        foreach (var row in panelController._modSettings)
        {
            if (row && !row.GetComponent<OptionRowFitter>())
                AttachTo(row);
        }
    }

    private static void AttachTo(ModSetting row)
    {
        var text = NameTextOf(row);
        if (!text)
            return;

        var fitter = row.gameObject.AddComponent<OptionRowFitter>();
        fitter._row = (RectTransform)row.transform;
        fitter._text = text;
        fitter._label = text.rectTransform;
        fitter._checkbox = row is ModSettingsBool checkboxRow ? checkboxRow.checkBox : null;
        fitter._restartRequired = IsRestartRequired(row);
    }

    private void LateUpdate()
    {
        if (_row.rect.width < 1)
            return;

        enabled = false;

        if (_checkbox)
            AlignCheckboxWithOtherControls();

        if (_label.anchorMax.x > 0.5f)
            EndLabelBeforeControls();

        if (_restartRequired)
            AddRestartIcon();

        FitText();
    }

    // Risk Of Options' checkbox row places the box about 100px in from the right edge, where every other control ends a
    // few pixels from it.
    private void AlignCheckboxWithOtherControls()
    {
        var corners = new Vector3[4];
        _checkbox.rectTransform.GetWorldCorners(corners);
        var checkboxRight = _row.InverseTransformPoint(corners[2]).x;
        var shift = _row.rect.xMax - CheckboxRightInset - checkboxRight;

        var container = ChildOfRowContaining(_checkbox.transform);
        if (container)
            container.anchoredPosition += new Vector2(shift, 0);
    }

    private RectTransform ChildOfRowContaining(Transform descendant)
    {
        var current = descendant;
        while (current && current.parent != _row)
            current = current.parent;

        return current as RectTransform;
    }

    private void EndLabelBeforeControls()
    {
        var controlsLeft = float.MaxValue;
        foreach (Transform child in _row)
        {
            if (child == _label || !child.gameObject.activeInHierarchy || DecorationNames.Contains(child.name))
                continue;

            foreach (var graphic in child.GetComponentsInChildren<Graphic>())
                controlsLeft = Mathf.Min(controlsLeft, LeftEdgeInRow(graphic.rectTransform));
        }

        if (controlsLeft == float.MaxValue)
            return;

        var labelRight = _row.rect.xMax + _label.offsetMax.x;
        var allowedRight = controlsLeft - GapBeforeControl;
        if (allowedRight < labelRight)
            _label.offsetMax = new Vector2(_label.offsetMax.x - (labelRight - allowedRight), _label.offsetMax.y);
    }

    private float LeftEdgeInRow(RectTransform rect)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return _row.InverseTransformPoint(corners[0]).x;
    }

    private void AddRestartIcon()
    {
        _label.offsetMin = new Vector2(_label.offsetMin.x + IconSize + IconGap, _label.offsetMin.y);

        var iconObject = new GameObject("Restart Required Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.layer = gameObject.layer;

        var rect = (RectTransform)iconObject.transform;
        rect.SetParent(_label, false);
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.sizeDelta = new Vector2(IconSize, IconSize);
        rect.anchoredPosition = new Vector2(-(IconSize + IconGap), 0);

        var image = iconObject.GetComponent<Image>();
        image.sprite = RooAssets.Load<Sprite>(RestartIconPath);
        image.preserveAspect = true;
        image.color = IconColor;
        image.raycastTarget = false;
    }

    private void FitText()
    {
        _text.enableAutoSizing = false;
        _text.enableWordWrapping = true;
        _text.overflowMode = TextOverflowModes.Overflow;

        var neededHeight = _text.GetPreferredValues(_text.text, _label.rect.width, 0).y;
        var missingHeight = neededHeight - _label.rect.height;
        if (missingHeight > 0)
            GrowRow(missingHeight);
    }

    private void GrowRow(float extraHeight)
    {
        var layoutElement = _row.GetComponent<LayoutElement>();
        if (!layoutElement)
            layoutElement = _row.gameObject.AddComponent<LayoutElement>();

        var height = _row.rect.height + extraHeight;
        layoutElement.minHeight = Mathf.Max(layoutElement.minHeight, height);
        layoutElement.preferredHeight = Mathf.Max(layoutElement.preferredHeight, height);
        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)_row.parent);
    }

    private static TMP_Text NameTextOf(ModSetting row)
    {
        if (!row.nameLabel)
            return null;

        var text = row.nameLabel.GetComponent<TMP_Text>();
        return text ? text : row.nameLabel.GetComponentInChildren<TMP_Text>(true);
    }

    private static bool IsRestartRequired(ModSetting row)
    {
        if (string.IsNullOrEmpty(row.settingToken))
            return false;

        try
        {
            return ModSettingsManager.OptionCollection.GetOption(row.settingToken).GetConfig().restartRequired;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }
}
