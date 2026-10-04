using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptionsExtender.Resilience;

namespace RiskOfOptionsExtender.Generation;

internal static class OptionIdentifiers
{
    public static bool IsTaken(string modGuid, BaseOption option, string name)
    {
        var identifier = Predict(modGuid, option.GetConfig().category, name, option.OptionTypeName);
        return ModSettingsManager.OptionCollection._identifierModGuidMap.ContainsKey(identifier);
    }

    public static void VerifyPrediction(BaseOption option)
    {
        var predicted = Predict(option.ModGuid, option.Category, option.Name, option.OptionTypeName);
        if (option.Identifier != predicted)
            throw new IncompatibilityException($"Risk Of Options identified an option as \"{option.Identifier}\" instead of \"{predicted}\", so duplicate options can no longer be detected.");
    }

    // Mirrors ModSettingsManager.AddOption. Risk Of Options adds an option to its category before rejecting a duplicate
    // identifier, which leaves a broken row behind, so duplicates have to be caught before adding.
    private static string Predict(string modGuid, string category, string name, string optionTypeName)
    {
        return $"{modGuid}.{category}.{name}.{optionTypeName}".Replace(" ", "_").ToUpper();
    }
}
