using RiskOfOptions.OptionConfigs;

namespace RiskOfOptionsExtender.Generation;

internal readonly struct OptionDetails(string category, string name, string description, bool restartRequired)
{
    public T ApplyTo<T>(T config) where T : BaseOptionConfig
    {
        config.category = category;
        config.name = name;
        config.description = description;
        config.restartRequired = restartRequired;
        return config;
    }
}
