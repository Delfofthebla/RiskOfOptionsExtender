namespace RiskOfOptionsExtender.Generation;

internal readonly struct OptionLabels(string category, string name, string description)
{
    public string Category { get; } = category;

    public string Name { get; } = name;

    public string Description { get; } = description;
}
