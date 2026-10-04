using System;
using System.Collections.Generic;
using RiskOfOptions;
using RiskOfOptions.Lib;
using RiskOfOptionsExtender.Generation;

namespace RiskOfOptionsExtender.UI.OptionRows;

internal static class RestartNotes
{
    private const string RiskOfOptionsTokenPrefix = "RISK_OF_OPTIONS";
    private const string Note = "<color=#FF8080>Requires a restart to take effect.</color>";

    private static readonly Dictionary<string, string> _originalDescriptions = [];

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

                var description = option.Description ?? "";
                _originalDescriptions[token] = description;
                LanguageApi.Add(token, description.Length == 0 ? Note : $"{description}\n\n{Note}");
            }
        }
    }

    public static void Remove()
    {
        foreach (var noted in _originalDescriptions)
            LanguageApi.Add(noted.Key, noted.Value);

        _originalDescriptions.Clear();
    }
}
