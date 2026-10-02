using System;
using RiskOfOptions;
using RiskOfOptions.Lib;

namespace RiskOfOptionsExtender.Generation;

internal static class RestartNotes
{
    private const string RiskOfOptionsTokenPrefix = "RISK_OF_OPTIONS";
    private const string Note = "<color=#FF8080>Requires a restart to take effect.</color>";

    public static void Apply()
    {
        foreach (var collection in ModSettingsManager.OptionCollection)
        {
            foreach (var option in RegisteredEntries.OptionsOf(collection))
            {
                if (!option.GetConfig().restartRequired)
                    continue;

                var token = option.GetDescriptionToken();
                if (!token.StartsWith(RiskOfOptionsTokenPrefix, StringComparison.Ordinal))
                    continue;

                var description = option.Description;
                LanguageApi.Add(token, string.IsNullOrEmpty(description) ? Note : $"{description}\n\n{Note}");
            }
        }
    }
}
