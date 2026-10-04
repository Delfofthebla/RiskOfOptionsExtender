using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx.Configuration;
using RiskOfOptionsExtender.Discovery;
using UnityEngine;

namespace RiskOfOptionsExtender.Generation;

internal static class ProxyEntries
{
    private const string Section = "Proxies";

    private static readonly HashSet<Type> SmallIntegers = [typeof(sbyte), typeof(byte), typeof(short), typeof(ushort)];
    private static readonly HashSet<Type> LargeIntegers = [typeof(uint), typeof(long), typeof(ulong)];

    private static readonly Dictionary<ConfigEntryBase, ConfigEntryBase> _sourceByProxy = [];
    private static readonly Dictionary<ConfigEntryBase, ConfigEntryBase> _proxyBySource = [];
    private static ConfigFile _file;
    private static int _nextKey;

    public static ConfigEntryBase SourceOf(ConfigEntryBase proxy)
    {
        return _sourceByProxy.TryGetValue(proxy, out var source) ? source : null;
    }

    public static bool CanProxy(ConfigEntryBase source)
    {
        return BinderFor(source) != null;
    }

    public static ConfigEntryBase GetOrCreate(ConfigEntryBase source)
    {
        if (_proxyBySource.TryGetValue(source, out var existing))
            return existing;

        var bind = BinderFor(source);
        if (bind == null)
            return null;

        var proxy = bind(source);
        _proxyBySource[source] = proxy;
        _sourceByProxy[proxy] = source;
        return proxy;
    }

    private static Func<ConfigEntryBase, ConfigEntryBase> BinderFor(ConfigEntryBase source)
    {
        var type = source.SettingType;

        if (type == typeof(double) || type == typeof(decimal))
            return BindFloat;

        if (SmallIntegers.Contains(type))
            return BindInt;

        if (LargeIntegers.Contains(type) && NumericRange.Of(source)?.FitsInInt == true)
            return BindInt;

        if (type == typeof(KeyCode))
            return BindKeyCode;

        if (TomlTypeConverter.CanConvert(type))
            return BindText;

        return null;
    }

    private static ConfigEntryBase BindFloat(ConfigEntryBase source)
    {
        var type = source.SettingType;
        var range = NumericRange.Of(source);
        var acceptableValues = range == null ? null : new AcceptableValueRange<float>((float)range.Min, (float)range.Max);

        return Bind(source, value => Convert.ToSingle(value, CultureInfo.InvariantCulture),
            number => Convert.ChangeType(number.ToString(CultureInfo.InvariantCulture), type, CultureInfo.InvariantCulture), acceptableValues);
    }

    private static ConfigEntryBase BindInt(ConfigEntryBase source)
    {
        var type = source.SettingType;
        var range = NumericRange.Of(source);
        var acceptableValues = range == null ? null : new AcceptableValueRange<int>((int)range.Min, (int)range.Max);

        return Bind(source, value => Convert.ToInt32(value, CultureInfo.InvariantCulture),
            number => Convert.ChangeType(number, type, CultureInfo.InvariantCulture), acceptableValues);
    }

    private static ConfigEntryBase BindKeyCode(ConfigEntryBase source)
    {
        return Bind(source, value => new KeyboardShortcut((KeyCode)value), shortcut => shortcut.MainKey, null);
    }

    private static ConfigEntryBase BindText(ConfigEntryBase source)
    {
        var type = source.SettingType;
        return Bind(source, value => TomlTypeConverter.ConvertToString(value, type), text => TomlTypeConverter.ConvertToValue(text, type), null);
    }

    private static ConfigEntry<TProxy> Bind<TProxy>(ConfigEntryBase source, Func<object, TProxy> toProxy, Func<TProxy, object> toSource, AcceptableValueBase acceptableValues)
    {
        _file ??= UnsavedConfigFiles.Create("Proxies");

        var definition = new ConfigDefinition(Section, (_nextKey++).ToString(CultureInfo.InvariantCulture));
        var description = new ConfigDescription(source.Description.Description, acceptableValues);
        var proxy = _file.Bind(definition, toProxy(source.DefaultValue), description);

        try
        {
            new ProxyLink<TProxy>(source, proxy, toProxy, toSource).Start();
        }
        catch
        {
            _file.Remove(definition);
            throw;
        }

        return proxy;
    }
}
