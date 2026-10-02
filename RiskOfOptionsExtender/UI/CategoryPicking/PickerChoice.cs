namespace RiskOfOptionsExtender.UI.CategoryPicking;

internal readonly struct PickerChoice
{
    private PickerChoice(string text, int categoryIndex, bool isHeader)
    {
        Text = text;
        CategoryIndex = categoryIndex;
        IsHeader = isHeader;
    }

    public string Text { get; }

    public int CategoryIndex { get; }

    public bool IsHeader { get; }

    public static PickerChoice Header(string text)
    {
        return new PickerChoice(text, -1, true);
    }

    public static PickerChoice Category(string text, int categoryIndex)
    {
        return new PickerChoice(text, categoryIndex, false);
    }
}
