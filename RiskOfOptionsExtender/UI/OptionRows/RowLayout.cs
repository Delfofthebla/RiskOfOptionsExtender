using UnityEngine;

namespace RiskOfOptionsExtender.UI.OptionRows;

internal readonly record struct RowLayout(float RowWidth, float LabelWidth, float ControlsLeft, int LineCount)
{
    private static readonly float Tolerance = 0.5f;

    public bool Matches(RowLayout other) => IsClose(RowWidth, other.RowWidth) && IsClose(LabelWidth, other.LabelWidth) && IsClose(ControlsLeft, other.ControlsLeft) && LineCount == other.LineCount;

    private static bool IsClose(float a, float b) => Mathf.Abs(a - b) < Tolerance;
}
