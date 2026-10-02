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

    private static readonly Dictionary<ConfigEntryBase, ConfigEntryBase> _sourceByProxy = [];
    private static readonly Dictionary<ConfigEntryBase, ConfigEntryBase> _proxyBySource = [];
    private static ConfigFile _file;
    private static int _nextKey;

    public static ConfigEntryBase SourceOf(ConfigEntryBase proxy)
    {
        return _sourceByProxy.TryGetValue(proxy, out var source) ? source : null;
    }

    public static ConfigEntryBase GetOrCreate(ConfigEntryBase source)
    {
        if (_proxyBySource.TryGetValue(source, out var existing))
            return existing;

        var proxy = Create(source);
        if (proxy == null)
            return null;

        _proxyBySource[source] = proxy;
        _sourceByProxy[proxy] = source;
        return proxy;
    }

    private static ConfigEntryBase Create(ConfigEntryBase source)
    {
        var type = source.SettingType;

        if (type == typeof(double) || type == typeof(decimal))
            return CreateFloat(source);

        if (type == typeof(sbyte) || type == typeof(byte) || type == typeof(short) || type == typeof(ushort))
            return CreateInt(source);

        if ((type == typeof(uint) || type == typeof(long) || type == typeof(ulong)) && NumericRange.Of(source)?.FitsInInt == true)
            return CreateInt(source);

        if (type == typeof(KeyCode))
            return Bind(source, value => new KeyboardShortcut((KeyCode)value), shortcut => shortcut.MainKey, null);

        if (TomlTypeConverter.CanConvert(type))
            return Bind(source, value => TomlTypeConverter.ConvertToString(value, type), text => TomlTypeConverter.ConvertToValue(text, type), null);

        return null;
    }

    private static ConfigEntryBase CreateFloat(ConfigEntryBase source)
    {
        var type = source.SettingType;
        var range = NumericRange.Of(source);
        var acceptableValues = range == null ? null : new AcceptableValueRange<float>((float)range.Min, (float)range.Max);

        return Bind(source, value => Convert.ToSingle(value, CultureInfo.InvariantCulture),
            number => Convert.ChangeType(number.ToString(CultureInfo.InvariantCulture), type, CultureInfo.InvariantCulture), acceptableValues);
    }

    private static ConfigEntryBase CreateInt(ConfigEntryBase source)
    {
        var type = source.SettingType;
        var range = NumericRange.Of(source);
        var acceptableValues = range == null ? null : new AcceptableValueRange<int>((int)range.Min, (int)range.Max);

        return Bind(source, value => Convert.ToInt32(value, CultureInfo.InvariantCulture),
            number => Convert.ChangeType(number, type, CultureInfo.InvariantCulture), acceptableValues);
    }

    private static ConfigEntry<TProxy> Bind<TProxy>(ConfigEntryBase source, Func<object, TProxy> toProxy, Func<TProxy, object> toSource, AcceptableValueBase acceptableValues)
    {
        _file ??= UnsavedConfigFiles.Create("Proxies");

        var definition = new ConfigDefinition(Section, (_nextKey++).ToString(CultureInfo.InvariantCulture));
        var description = new ConfigDescription(source.Description.Description, acceptableValues);
        var proxy = _file.Bind(definition, toProxy(source.DefaultValue), description);

        new ProxyLink<TProxy>(source, proxy, toProxy, toSource).Start();
        return proxy;
    }
}
