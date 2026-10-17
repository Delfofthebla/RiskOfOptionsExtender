using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Discovery;

internal sealed class UnsavedConfigFile(string name)
{
    private static readonly HashSet<ConfigFile> _allFiles = [];

    private ConfigFile _file;
    private int _nextKey;

    public static bool IsUnsaved(ConfigFile file) => file != null && _allFiles.Contains(file);

    public ConfigEntry<T> Bind<T>(T defaultValue, ConfigDescription description)
    {
        _file ??= CreateFile();

        var key = (_nextKey++).ToString(CultureInfo.InvariantCulture);
        return _file.Bind(name, key, defaultValue, description);
    }

    public void Remove(ConfigEntryBase entry) => _file.Remove(entry.Definition);

    private ConfigFile CreateFile()
    {
        var path = Path.Combine(Paths.CachePath, $"RiskOfOptionsExtender.{name}.cfg");
        if (File.Exists(path))
            File.Delete(path);

        var file = new ConfigFile(path, false) { SaveOnConfigSet = false };
        _allFiles.Add(file);
        return file;
    }
}
