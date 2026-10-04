using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Settings;

internal sealed class GuidListSetting
{
    private readonly ConfigEntry<string> _entry;
    private HashSet<string> _guids;

    public GuidListSetting(ConfigEntry<string> entry)
    {
        _entry = entry;
        _guids = Parse(entry.Value);
        entry.SettingChanged += OnEntryChanged;
    }

    public event Action Changed;

    public bool Contains(string guid)
    {
        return _guids.Contains(guid);
    }

    public void Set(string guid, bool included)
    {
        if (included)
            _guids.Add(guid);
        else
            _guids.Remove(guid);

        Save();
    }

    public void Toggle(string guid)
    {
        Set(guid, !Contains(guid));
    }

    private void Save()
    {
        _entry.Value = string.Join(",", _guids.OrderBy(guid => guid, StringComparer.Ordinal));
    }

    private void OnEntryChanged(object sender, EventArgs args)
    {
        _guids = Parse(_entry.Value);
        Changed?.Invoke();
    }

    private static HashSet<string> Parse(string commaSeparated)
    {
        return [.. commaSeparated.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(guid => guid.Trim())];
    }
}
