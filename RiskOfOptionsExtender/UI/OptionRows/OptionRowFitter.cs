using System.Collections.Generic;
using RiskOfOptions.Components.Options;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Resilience;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.OptionRows;

internal sealed class OptionRowFitter : MonoBehaviour
{
    private const float GapBeforeControl = 12f;
    private const float CheckboxRightInset = 12f;
    private static readonly HashSet<string> DecorationNames = ["BaseOutline", "HoverOutline", "Mod Settings Indicator"];
    private static readonly HashSet<OptionRowFitter> _liveFitters = [];

    private RectTransform _row;
    private RectTransform _label;
    private TMP_Text _text;
    private Image _checkbox;
    private FeatureGuard _guard;
    private bool _fitted;
    private OriginalRowState _original;
    private RectTransform _movedCheckboxContainer;
    private Vector2 _checkboxContainerOriginalPosition;
    private LayoutElement _addedLayoutElement;

    public static void RevertAll()
    {
        foreach (var fitter in new List<OptionRowFitter>(_liveFitters))
            fitter.Revert();
    }

    public static void AttachToRows(ModOptionPanelController panelController, FeatureGuard guard)
    {
        foreach (var row in panelController._modSettings)
        {
            if (row && !row.GetComponent<OptionRowFitter>())
                AttachTo(row, guard);
        }
    }

    private static void AttachTo(ModSetting row, FeatureGuard guard)
    {
        var text = row.NameText();
        if (!text)
            return;

        var fitter = row.gameObject.AddComponent<OptionRowFitter>();
        fitter._row = (RectTransform)row.transform;
        fitter._text = text;
        fitter._label = text.rectTransform;
        fitter._checkbox = row is ModSettingsBool checkboxRow ? checkboxRow.checkBox : null;
        fitter._guard = guard;
        _liveFitters.Add(fitter);
    }

    private void OnDestroy()
    {
        _liveFitters.Remove(this);
    }

    private void Revert()
    {
        if (_fitted)
            RestoreRow();

        Destroy(this);
    }

    private void RestoreRow()
    {
        _original.RestoreLabel(_label, _text);

        if (_movedCheckboxContainer)
            _movedCheckboxContainer.anchoredPosition = _checkboxContainerOriginalPosition;

        if (_addedLayoutElement)
            Destroy(_addedLayoutElement);
        else if (_row.TryGetComponent<LayoutElement>(out var layoutElement))
            _original.RestoreHeight(layoutElement);

        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)_row.parent);
    }

    private void LateUpdate()
    {
        if (_row.rect.width < 1)
            return;

        enabled = false;
        _guard.Run("fit an option row", Fit);
    }

    private void Fit()
    {
        _original = OriginalRowState.Capture(_row, _label, _text);
        _fitted = true;

        if (_checkbox)
            AlignCheckboxWithOtherControls();

        if (_label.anchorMax.x > 0.5f)
            EndLabelBeforeControls();

        FitText();
    }

    // Risk Of Options' checkbox row places the box about 100px in from the right edge, where every other control ends a
    // few pixels from it.
    private void AlignCheckboxWithOtherControls()
    {
        var checkboxRight = _row.InverseTransformPoint(_checkbox.rectTransform.WorldTopRight()).x;
        var shift = _row.rect.xMax - CheckboxRightInset - checkboxRight;

        var container = ChildOfRowContaining(_checkbox.transform);
        if (!container)
            return;

        _checkboxContainerOriginalPosition = container.anchoredPosition;
        _movedCheckboxContainer = container;
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
                controlsLeft = Mathf.Min(controlsLeft, _row.InverseTransformPoint(graphic.rectTransform.WorldBottomLeft()).x);
        }

        if (controlsLeft == float.MaxValue)
            return;

        var labelRight = _row.rect.xMax + _label.offsetMax.x;
        var allowedRight = controlsLeft - GapBeforeControl;
        if (allowedRight < labelRight)
            _label.offsetMax = new Vector2(_label.offsetMax.x - (labelRight - allowedRight), _label.offsetMax.y);
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
        if (!_row.TryGetComponent<LayoutElement>(out var layoutElement))
            layoutElement = _addedLayoutElement = _row.gameObject.AddComponent<LayoutElement>();

        var height = _row.rect.height + extraHeight;
        layoutElement.minHeight = Mathf.Max(layoutElement.minHeight, height);
        layoutElement.preferredHeight = Mathf.Max(layoutElement.preferredHeight, height);
        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)_row.parent);
    }
}
