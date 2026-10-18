using System;
using System.Collections.Generic;
using System.Linq;
using RiskOfOptions.Components.Options;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Resilience;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.OptionRows;

internal sealed class OptionRowFitter : MonoBehaviour
{
    private static readonly float GapBeforeControl = 12f;
    private static readonly float CheckboxRightInset = 12f;
    private static readonly HashSet<OptionRowFitter> _liveFitters = [];
    private static int _layoutSettledFrame = -1;

    private readonly List<Graphic> _controlGraphics = [];
    private RectTransform _row;
    private RectTransform _label;
    private TMP_Text _text;
    private Image _checkbox;
    private FeatureGuard _guard;
    private Action _fitIfLayoutChanged;
    private bool _fitted;
    private OriginalRowState _original;
    private RowLayout _fittedLayout;
    private RectTransform _movedCheckboxContainer;
    private Vector2 _checkboxContainerOriginalPosition;
    private LayoutElement _addedLayoutElement;

    public static void RevertAll()
    {
        foreach (var fitter in _liveFitters.ToList())
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
        fitter._checkbox = row is ModSettingsBool checkboxRow
            ? checkboxRow.checkBox
            : null;
        fitter._guard = guard;
        _liveFitters.Add(fitter);
    }

    private void Awake() => _fitIfLayoutChanged = FitIfLayoutChanged;

    private void OnDestroy() => _liveFitters.Remove(this);

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

        _guard.Run("fit an option row", _fitIfLayoutChanged);
    }

    private void FitIfLayoutChanged()
    {
        if (!_fitted)
            FitFirstTime();
        else if (!_fittedLayout.Matches(CurrentLayout()))
            Fit();
    }

    private void FitFirstTime()
    {
        SettleCanvasLayout();
        _original = OriginalRowState.Capture(_row, _label, _text);
        _fitted = true;

        CollectControlGraphics();
        Fit();
    }

    // Rows Risk Of Options copies from the game's settings panel arrive already sized, before their nested layout groups place
    // their controls.
    private static void SettleCanvasLayout()
    {
        if (_layoutSettledFrame == Time.frameCount)
            return;

        _layoutSettledFrame = Time.frameCount;
        Canvas.ForceUpdateCanvases();
    }

    private void CollectControlGraphics()
    {
        HashSet<Graphic> movingGraphics = [.. HoverImages(), .. SliderHandles()];
        _controlGraphics.AddRange(_row.GetComponentsInChildren<Graphic>().Where(graphic => IsInsideControl(graphic.transform) && !movingGraphics.Contains(graphic)));
    }

    private IEnumerable<Graphic> HoverImages() => _row.GetComponentsInChildren<HGButton>().Select(button => button.imageOnHover);

    private IEnumerable<Graphic> SliderHandles() => _row.GetComponentsInChildren<Slider>()
        .Where(slider => slider.handleRect)
        .SelectMany(slider => slider.handleRect.GetComponentsInChildren<Graphic>());

    private bool IsInsideControl(Transform descendant)
    {
        var control = descendant.GetComponentInParent<Selectable>();
        return control && control.transform != _row && control.transform.IsChildOf(_row);
    }

    private void Fit()
    {
        if (_checkbox)
            AlignCheckboxWithOtherControls();

        var controlsLeft = ControlsLeft();
        if (controlsLeft < float.MaxValue)
            EndLabelBeforeControls(controlsLeft);

        FitText();
        _fittedLayout = CurrentLayout();
    }

    private RowLayout CurrentLayout() => new(_row.rect.width, _label.rect.width, ControlsLeft(), _text.textInfo.lineCount);

    private float ControlsLeft()
    {
        var left = float.MaxValue;
        foreach (var graphic in _controlGraphics)
        {
            if (graphic)
                left = Mathf.Min(left, _row.InverseTransformPoint(graphic.rectTransform.WorldBottomLeft()).x);
        }

        return left;
    }

    private void AlignCheckboxWithOtherControls()
    {
        var container = ChildOfRowContaining(_checkbox.transform);
        if (!container)
            return;

        if (!_movedCheckboxContainer)
        {
            _movedCheckboxContainer = container;
            _checkboxContainerOriginalPosition = container.anchoredPosition;
        }

        var checkboxRight = _row.InverseTransformPoint(_checkbox.rectTransform.WorldTopRight()).x;
        var shift = _row.rect.xMax - CheckboxRightInset - checkboxRight;
        container.anchoredPosition += new Vector2(shift, 0);
    }

    private RectTransform ChildOfRowContaining(Transform descendant)
    {
        var current = descendant;
        while (current && current.parent != _row)
            current = current.parent;

        return current as RectTransform;
    }

    private void EndLabelBeforeControls(float controlsLeft)
    {
        var labelRight = _row.InverseTransformPoint(_label.WorldTopRight()).x;
        var allowedRight = controlsLeft - GapBeforeControl;
        _label.offsetMax += new Vector2(allowedRight - labelRight, 0);
    }

    private void FitText()
    {
        _text.enableAutoSizing = false;
        _text.enableWordWrapping = true;
        _text.overflowMode = TextOverflowModes.Overflow;

        // GetPreferredValues can wrap a name onto one line fewer than TextMeshPro renders it.
        _text.ForceMeshUpdate();
        var labelInset = _row.rect.height - _label.rect.height;
        FitRowHeight(_text.textBounds.size.y + labelInset);
    }

    private void FitRowHeight(float neededHeight)
    {
        var hasLayoutElement = _row.TryGetComponent<LayoutElement>(out var layoutElement);
        if (neededHeight <= _original.RowHeight)
        {
            if (hasLayoutElement)
                _original.RestoreHeight(layoutElement);

            return;
        }

        if (!hasLayoutElement)
            layoutElement = _addedLayoutElement = _row.gameObject.AddComponent<LayoutElement>();

        layoutElement.minHeight = Mathf.Max(_original.MinHeight, neededHeight);
        layoutElement.preferredHeight = Mathf.Max(_original.PreferredHeight, neededHeight);
    }
}
