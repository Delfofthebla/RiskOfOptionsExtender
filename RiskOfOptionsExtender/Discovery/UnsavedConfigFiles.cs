using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Discovery;

internal static class UnsavedConfigFiles
{
    private static readonly HashSet<ConfigFile> _files = [];

    public static ConfigFile Create(string name)
    {
        var path = Path.Combine(Paths.CachePath, $"RiskOfOptionsExtender.{name}.cfg");
        if (File.Exists(path))
            File.Delete(path);

        var file = new ConfigFile(path, false) { SaveOnConfigSet = false };
        _files.Add(file);
        return file;
    }

    public static bool Contains(ConfigFile file)
    {
        return file != null && _files.Contains(file);
    }
}
