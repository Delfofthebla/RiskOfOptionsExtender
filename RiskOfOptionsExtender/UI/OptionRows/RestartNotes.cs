using System;
using System.Collections.Generic;
using RiskOfOptions.Lib;
using RiskOfOptions.Options;
using RiskOfOptionsExtender.Generation;

namespace RiskOfOptionsExtender.UI.OptionRows;

internal static class RestartNotes
{
    private static readonly string RiskOfOptionsTokenPrefix = "RISK_OF_OPTIONS";
    private static readonly string Note = "<color=#FF8080>Requires a restart to take effect.</color>";

    private static readonly HashSet<string> _checkedIdentifiers = [];
    private static readonly Dictionary<string, string> _originalDescriptions = [];

    public static void Apply()
    {
        foreach (var option in RegisteredEntries.AllOptions())
        {
            if (_checkedIdentifiers.Add(option.Identifier))
                AddNoteIfRestartRequired(option);
        }
    }

    public static void Remove()
    {
        foreach (var noted in _originalDescriptions)
            LanguageApi.Add(noted.Key, noted.Value);

        _originalDescriptions.Clear();
        _checkedIdentifiers.Clear();
    }

    private static void AddNoteIfRestartRequired(BaseOption option)
    {
        if (!option.GetConfig().restartRequired)
            return;

        var token = option.GetDescriptionToken();
        if (!token.StartsWith(RiskOfOptionsTokenPrefix, StringComparison.Ordinal))
            return;

        var description = option.Description ?? "";
        _originalDescriptions[token] = description;
        LanguageApi.Add(token, WithRestartNote(description));
    }

    private static string WithRestartNote(string description)
    {
        if (description.Length == 0)
            return Note;

        return description
            + $"\n\n{Note}";
    }
}
