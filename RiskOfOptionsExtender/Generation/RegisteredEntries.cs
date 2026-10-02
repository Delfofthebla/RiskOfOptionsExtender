using System.Collections.Generic;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Containers;
using RiskOfOptions.Options;

namespace RiskOfOptionsExtender.Generation;

internal static class RegisteredEntries
{
    public static HashSet<ConfigEntryBase> Collect()
    {
        var entries = new HashSet<ConfigEntryBase>();
        foreach (var collection in ModSettingsManager.OptionCollection)
        {
            foreach (var option in OptionsOf(collection))
                AddEntryOf(option, entries);
        }

        return entries;
    }

    public static IEnumerable<BaseOption> OptionsOf(OptionCollection collection)
    {
        for (var categoryIndex = 0; categoryIndex < collection.CategoryCount; categoryIndex++)
        {
            var category = collection[categoryIndex];
            for (var optionIndex = 0; optionIndex < category.OptionCount; optionIndex++)
                yield return category[optionIndex];
        }
    }

    public static int CountOptions(OptionCollection collection)
    {
        var count = 0;
        for (var categoryIndex = 0; categoryIndex < collection.CategoryCount; categoryIndex++)
            count += collection[categoryIndex].OptionCount;

        return count;
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
