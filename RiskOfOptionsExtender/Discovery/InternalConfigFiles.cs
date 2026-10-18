using System;
using System.IO;
using System.Linq;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Discovery;

internal static class InternalConfigFiles
{
    private static readonly string BackupSuffix = "Backup";
    private static readonly string DoNotModifyMarker = "DO NOT MODIFY";

    public static bool Contains(ConfigFile file) => UnsavedConfigFile.IsUnsaved(file) || IsBackup(file) || IsMarkedDoNotModify(file);

    private static bool IsBackup(ConfigFile file) => Path.GetFileNameWithoutExtension(file.ConfigFilePath).EndsWith(BackupSuffix, StringComparison.OrdinalIgnoreCase);

    private static bool IsMarkedDoNotModify(ConfigFile file) => file.Keys.Any(definition => HasMarker(definition.Section) || HasMarker(definition.Key));

    private static bool HasMarker(string text) => text.IndexOf(DoNotModifyMarker, StringComparison.OrdinalIgnoreCase) >= 0;
}
