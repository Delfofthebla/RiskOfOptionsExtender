using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Containers;
using RiskOfOptions.Options;

namespace RiskOfOptionsExtender.Generation;

internal static class RegisteredEntries
{
    public static HashSet<ConfigEntryBase> Collect()
    {
        HashSet<ConfigEntryBase> entries = [];
        foreach (var option in AllOptions())
            AddEntryOf(option, entries);

        return entries;
    }

    public static IEnumerable<BaseOption> AllOptions() => ModSettingsManager.OptionCollection.SelectMany(OptionsOf);

    public static IEnumerable<BaseOption> OptionsOf(OptionCollection collection)
    {
        for (var categoryIndex = 0; categoryIndex < collection.CategoryCount; categoryIndex++)
        {
            var category = collection[categoryIndex];
            for (var optionIndex = 0; optionIndex < category.OptionCount; optionIndex++)
                yield return category[optionIndex];
        }
    }

    private static void AddEntryOf(BaseOption option, HashSet<ConfigEntryBase> entries)
    {
        var entry = option.ConfigEntry;
        if (entry == null)
            return;

        entries.Add(entry);

        var source = ProxyEntries.SourceOf(entry);
        if (source != null)
            entries.Add(source);
    }
}
