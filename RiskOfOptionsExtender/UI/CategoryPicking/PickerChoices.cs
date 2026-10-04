using System.Collections.Generic;
using System.Linq;
using RiskOfOptions.Containers;
using RiskOfOptionsExtender.Generation;
using RoR2;

namespace RiskOfOptionsExtender.UI.CategoryPicking;

internal static class PickerChoices
{
    public static List<PickerChoice> For(OptionCollection collection)
    {
        var groups = Enumerable.Range(0, collection.CategoryCount)
            .Select(index => CategoryGroups.FileLabelOf(collection.ModGuid, collection[index].name) ?? collection.ModName)
            .ToList();
        var isGrouped = groups.Distinct().Count() > 1;

        var choices = new List<PickerChoice>();
        string currentGroup = null;
        for (var index = 0; index < collection.CategoryCount; index++)
        {
            if (isGrouped && groups[index] != currentGroup)
            {
                currentGroup = groups[index];
                choices.Add(PickerChoice.Header(currentGroup));
            }

            choices.Add(PickerChoice.Category(Language.GetString(collection[index].NameToken), index));
        }

        return choices;
    }
}
