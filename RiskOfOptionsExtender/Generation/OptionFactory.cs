using System;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using TMPro;
using UnityEngine;

namespace RiskOfOptionsExtender.Generation;

internal static class OptionFactory
{
    public static BaseOption Create(ConfigEntryBase entry, OptionLabels labels, bool restartRequired)
    {
        var option = CreateNative(entry, labels, restartRequired);
        if (option != null)
            return option;

        var proxy = ProxyEntries.GetOrCreate(entry);
        return proxy == null ? null : CreateNative(proxy, labels, restartRequired);
    }

    private static BaseOption CreateNative(ConfigEntryBase entry, OptionLabels labels, bool restartRequired)
    {
        return entry switch
        {
            ConfigEntry<bool> toggle => new CheckBoxOption(toggle, Labelled(new CheckBoxConfig(), labels, restartRequired)),
            ConfigEntry<float> number => CreateFloat(number, labels, restartRequired),
            ConfigEntry<int> number => CreateInt(number, labels, restartRequired),
            ConfigEntry<string> text => new StringInputFieldOption(text, Labelled(TextFieldConfig(), labels, restartRequired)),
            ConfigEntry<KeyboardShortcut> shortcut => new KeyBindOption(shortcut, Labelled(new KeyBindConfig(), labels, restartRequired)),
            ConfigEntry<Color> color => new ColorOption(color, Labelled(new ColorOptionConfig(), labels, restartRequired)),
            _ when IsDropdownCompatibleEnum(entry.SettingType) => new ChoiceOption(entry, Labelled(new ChoiceConfig(), labels, restartRequired)),
            _ => null
        };
    }

    private static BaseOption CreateFloat(ConfigEntry<float> entry, OptionLabels labels, bool restartRequired)
    {
        if (entry.Description.AcceptableValues is AcceptableValueRange<float> range)
        {
            var sliderConfig = new SliderConfig { min = range.MinValue, max = range.MaxValue, FormatString = "{0:0.##}" };
            return new SliderOption(entry, Labelled(sliderConfig, labels, restartRequired));
        }

        return new FloatFieldOption(entry, Labelled(new FloatFieldConfig(), labels, restartRequired));
    }

    private static BaseOption CreateInt(ConfigEntry<int> entry, OptionLabels labels, bool restartRequired)
    {
        if (entry.Description.AcceptableValues is AcceptableValueRange<int> range)
            return new IntSliderOption(entry, Labelled(new IntSliderConfig { min = range.MinValue, max = range.MaxValue }, labels, restartRequired));

        return new IntFieldOption(entry, Labelled(new IntFieldConfig(), labels, restartRequired));
    }

    private static InputFieldConfig TextFieldConfig()
    {
        return new InputFieldConfig
        {
            submitOn = InputFieldConfig.SubmitEnum.OnExitOrSubmit,
            lineType = TMP_InputField.LineType.SingleLine,
            richText = false
        };
    }

    // RiskOfOptions' dropdown treats an enum's numeric value as its position in the list.
    private static bool IsDropdownCompatibleEnum(Type type)
    {
        if (!type.IsEnum || type.IsDefined(typeof(FlagsAttribute), false))
            return false;

        try
        {
            var values = Enum.GetValues(type).Cast<object>().Select(value => Convert.ToInt64(value, CultureInfo.InvariantCulture)).OrderBy(value => value).ToList();
            return values.Select((value, index) => value == index).All(isInPosition => isInPosition);
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    private static T Labelled<T>(T config, OptionLabels labels, bool restartRequired) where T : BaseOptionConfig
    {
        config.category = labels.Category;
        config.name = labels.Name;
        config.description = labels.Description;
        config.restartRequired = restartRequired;
        return config;
    }
}
