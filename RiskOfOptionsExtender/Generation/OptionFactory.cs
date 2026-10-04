using System;
using System.Collections.Generic;
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
    private static readonly Dictionary<Type, Func<ConfigEntryBase, OptionDetails, BaseOption>> NativeControls = new()
    {
        [typeof(bool)] = (entry, details) => new CheckBoxOption((ConfigEntry<bool>)entry, details.ApplyTo(new CheckBoxConfig())),
        [typeof(float)] = (entry, details) => CreateFloat((ConfigEntry<float>)entry, details),
        [typeof(int)] = (entry, details) => CreateInt((ConfigEntry<int>)entry, details),
        [typeof(string)] = (entry, details) => new StringInputFieldOption((ConfigEntry<string>)entry, details.ApplyTo(TextFieldConfig())),
        [typeof(KeyboardShortcut)] = (entry, details) => new KeyBindOption((ConfigEntry<KeyboardShortcut>)entry, details.ApplyTo(new KeyBindConfig())),
        [typeof(Color)] = (entry, details) => new ColorOption((ConfigEntry<Color>)entry, details.ApplyTo(new ColorOptionConfig()))
    };

    public static bool CanCreate(ConfigEntryBase entry)
    {
        return HasNativeControl(entry.SettingType) || ProxyEntries.CanProxy(entry);
    }

    public static BaseOption Create(ConfigEntryBase entry, OptionDetails details)
    {
        if (HasNativeControl(entry.SettingType))
            return CreateNative(entry, details);

        var proxy = ProxyEntries.GetOrCreate(entry);
        return proxy == null ? null : CreateNative(proxy, details);
    }

    private static bool HasNativeControl(Type type)
    {
        return NativeControls.ContainsKey(type) || IsDropdownCompatibleEnum(type);
    }

    private static BaseOption CreateNative(ConfigEntryBase entry, OptionDetails details)
    {
        return NativeControls.TryGetValue(entry.SettingType, out var create)
            ? create(entry, details)
            : new ChoiceOption(entry, details.ApplyTo(new ChoiceConfig()));
    }

    private static BaseOption CreateFloat(ConfigEntry<float> entry, OptionDetails details)
    {
        if (entry.Description.AcceptableValues is AcceptableValueRange<float> range)
            return new SliderOption(entry, details.ApplyTo(new SliderConfig { min = range.MinValue, max = range.MaxValue, FormatString = "{0:0.##}" }));

        return new FloatFieldOption(entry, details.ApplyTo(new FloatFieldConfig()));
    }

    private static BaseOption CreateInt(ConfigEntry<int> entry, OptionDetails details)
    {
        if (entry.Description.AcceptableValues is AcceptableValueRange<int> range)
            return new IntSliderOption(entry, details.ApplyTo(new IntSliderConfig { min = range.MinValue, max = range.MaxValue }));

        return new IntFieldOption(entry, details.ApplyTo(new IntFieldConfig()));
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
            return values.SequenceEqual(Enumerable.Range(0, values.Count).Select(position => (long)position));
        }
        catch (OverflowException)
        {
            return false;
        }
    }
}
