using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.OptionRows;

internal readonly struct OriginalRowState
{
    private readonly Vector2 _labelOffsetMax;
    private readonly bool _wordWrapping;
    private readonly TextOverflowModes _overflowMode;
    private readonly bool _autoSizing;
    private readonly float _minHeight;
    private readonly float _preferredHeight;

    private OriginalRowState(RectTransform row, RectTransform label, TMP_Text text)
    {
        _labelOffsetMax = label.offsetMax;
        _wordWrapping = text.enableWordWrapping;
        _overflowMode = text.overflowMode;
        _autoSizing = text.enableAutoSizing;
        _minHeight = 0;
        _preferredHeight = 0;

        if (row.TryGetComponent<LayoutElement>(out var layoutElement))
        {
            _minHeight = layoutElement.minHeight;
            _preferredHeight = layoutElement.preferredHeight;
        }
    }

    public static OriginalRowState Capture(RectTransform row, RectTransform label, TMP_Text text)
    {
        return new OriginalRowState(row, label, text);
    }

    public void RestoreLabel(RectTransform label, TMP_Text text)
    {
        label.offsetMax = _labelOffsetMax;
        text.enableWordWrapping = _wordWrapping;
        text.overflowMode = _overflowMode;
        text.enableAutoSizing = _autoSizing;
    }

    public void RestoreHeight(LayoutElement layoutElement)
    {
        layoutElement.minHeight = _minHeight;
        layoutElement.preferredHeight = _preferredHeight;
    }
}
