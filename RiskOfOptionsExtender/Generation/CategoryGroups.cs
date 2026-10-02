using System.Collections.Generic;

namespace RiskOfOptionsExtender.Generation;

internal static class CategoryGroups
{
    private static readonly Dictionary<(string ModGuid, string Category), string> _fileLabels = [];

    public static void Record(string modGuid, string category, string fileLabel)
    {
        if (!_fileLabels.ContainsKey((modGuid, category)))
            _fileLabels[(modGuid, category)] = fileLabel;
    }

    public static string FileLabelOf(string modGuid, string category)
    {
        return _fileLabels.TryGetValue((modGuid, category), out var fileLabel) ? fileLabel : null;
    }
}
