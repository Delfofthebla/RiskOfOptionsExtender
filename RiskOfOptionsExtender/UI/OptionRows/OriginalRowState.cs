using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfOptionsExtender.UI.OptionRows;

internal readonly record struct OriginalRowState(Vector2 LabelOffsetMax, bool WordWrapping, TextOverflowModes OverflowMode, bool AutoSizing, float MinHeight, float PreferredHeight)
{
    public static OriginalRowState Capture(RectTransform row, RectTransform label, TMP_Text text)
    {
        var hasLayoutElement = row.TryGetComponent<LayoutElement>(out var layoutElement);
        var minHeight = hasLayoutElement ? layoutElement.minHeight : 0;
        var preferredHeight = hasLayoutElement ? layoutElement.preferredHeight : 0;

        return new(label.offsetMax, text.enableWordWrapping, text.overflowMode, text.enableAutoSizing, minHeight, preferredHeight);
    }

    public void RestoreLabel(RectTransform label, TMP_Text text)
    {
        label.offsetMax = LabelOffsetMax;
        text.enableWordWrapping = WordWrapping;
        text.overflowMode = OverflowMode;
        text.enableAutoSizing = AutoSizing;
    }

    public void RestoreHeight(LayoutElement layoutElement)
    {
        layoutElement.minHeight = MinHeight;
        layoutElement.preferredHeight = PreferredHeight;
    }
}
