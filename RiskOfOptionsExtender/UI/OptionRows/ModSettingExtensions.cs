using System.Collections.Generic;
using RiskOfOptions;
using RiskOfOptions.Components.Options;
using TMPro;

namespace RiskOfOptionsExtender.UI.OptionRows;

internal static class ModSettingExtensions
{
    public static TMP_Text NameText(this ModSetting row)
    {
        if (!row.nameLabel)
            return null;

        var text = row.nameLabel.GetComponent<TMP_Text>();
        return text ? text : row.nameLabel.GetComponentInChildren<TMP_Text>(true);
    }

    public static bool IsRestartRequired(this ModSetting row)
    {
        if (string.IsNullOrEmpty(row.settingToken))
            return false;

        try
        {
            return ModSettingsManager.OptionCollection.GetOption(row.settingToken).GetConfig().restartRequired;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }
}
