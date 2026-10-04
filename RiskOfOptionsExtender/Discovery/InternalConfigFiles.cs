using System;
using System.IO;
using System.Linq;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Discovery;

// Some mods keep a second config file as private bookkeeping, such as the "auto config sync" code shared by
// StageAesthetic, Sandswept and ArtificerExtended, which stores the previous version's defaults in a *.Backup.cfg
// marked "DO NOT MODIFY". Those files aren't settings and must not be edited from the menu.
internal static class InternalConfigFiles
{
    private const string BackupSuffix = "Backup";
    private const string DoNotModifyMarker = "DO NOT MODIFY";

    public static bool Contains(ConfigFile file)
    {
        return UnsavedConfigFiles.Contains(file) || IsBackup(file) || IsMarkedDoNotModify(file);
    }

    private static bool IsBackup(ConfigFile file)
    {
        return Path.GetFileNameWithoutExtension(file.ConfigFilePath).EndsWith(BackupSuffix, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMarkedDoNotModify(ConfigFile file)
    {
        return file.Keys.Any(definition => HasMarker(definition.Section) || HasMarker(definition.Key));
    }

    private static bool HasMarker(string text)
    {
        return text.IndexOf(DoNotModifyMarker, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
