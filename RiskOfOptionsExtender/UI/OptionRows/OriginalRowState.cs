using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.OptionRows;

internal struct OriginalRowState
{
    public Vector2 LabelOffsetMin;
    public Vector2 LabelOffsetMax;
    public bool WordWrapping;
    public TextOverflowModes OverflowMode;
    public bool AutoSizing;
    public float MinHeight;
    public float PreferredHeight;
    public Vector2 CheckboxContainerPosition;

    public static OriginalRowState Capture(RectTransform row, RectTransform label, TMP_Text text)
    {
        var state = new OriginalRowState
        {
            LabelOffsetMin = label.offsetMin,
            LabelOffsetMax = label.offsetMax,
            WordWrapping = text.enableWordWrapping,
            OverflowMode = text.overflowMode,
            AutoSizing = text.enableAutoSizing
        };

        if (row.TryGetComponent<LayoutElement>(out var layoutElement))
        {
            state.MinHeight = layoutElement.minHeight;
            state.PreferredHeight = layoutElement.preferredHeight;
        }

        return state;
    }
}
