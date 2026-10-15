using RiskOfOptions.OptionConfigs;

namespace RiskOfOptionsExtender.Generation;

internal sealed record OptionDetails(string Category, string Name, string Description, bool RestartRequired)
{
    public T ApplyTo<T>(T config) where T : BaseOptionConfig
    {
        config.category = Category;
        config.name = Name;
        config.description = Description;
        config.restartRequired = RestartRequired;
        return config;
    }
}
