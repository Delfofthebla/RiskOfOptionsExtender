using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptionsExtender.Discovery;

namespace RiskOfOptionsExtender.Settings;

internal sealed class SkipToggles(ExtenderSettings settings)
{
    private readonly UnsavedConfigFile _file = new("SkipToggles");
    private readonly HashSet<string> _guidsWithToggle = [];

    public void AddFor(IEnumerable<PluginInfo> plugins)
    {
        var newPlugins = plugins.Where(plugin => !_guidsWithToggle.Contains(plugin.Metadata.GUID)).OrderBy(plugin => plugin.Metadata.Name, StringComparer.CurrentCultureIgnoreCase);

        foreach (var plugin in newPlugins)
            Add(plugin);
    }

    private void Add(PluginInfo plugin)
    {
        var guid = plugin.Metadata.GUID;
        var name = plugin.Metadata.Name;

        var toggle = _file.Bind(false, new ConfigDescription($"Don't add the options \"{name}\" leaves out of this menu."));
        toggle.Value = settings.SkippedMods.Contains(guid);
        toggle.SettingChanged += (_, _) => settings.SkippedMods.Set(guid, toggle.Value);
        settings.SkippedMods.Changed += () => toggle.Value = settings.SkippedMods.Contains(guid);
        _guidsWithToggle.Add(guid);

        var config = new CheckBoxConfig
        {
            name = $"Skip {name}",
            category = ExtenderSettings.FillInSection,
            restartRequired = true,
            checkIfDisabled = () => settings.NeverFillIn.Value
        };

        ExtenderSettings.AddOwnOption(new CheckBoxOption(toggle, config));
    }
}
