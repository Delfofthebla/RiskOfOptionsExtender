namespace RiskOfOptionsExtender.UI.CategoryPicking;

internal sealed record PickerChoice(string Text, int CategoryIndex, bool IsHeader)
{
    public static PickerChoice Header(string text) => new(text, -1, true);

    public static PickerChoice Category(string text, int categoryIndex) => new(text, categoryIndex, false);
}
