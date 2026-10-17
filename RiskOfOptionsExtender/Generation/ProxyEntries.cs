using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx.Configuration;
using RiskOfOptionsExtender.Discovery;
using UnityEngine;

namespace RiskOfOptionsExtender.Generation;

internal static class ProxyEntries
{
    private static readonly HashSet<Type> SmallIntegers = [typeof(sbyte), typeof(byte), typeof(short), typeof(ushort)];
    private static readonly HashSet<Type> LargeIntegers = [typeof(uint), typeof(long), typeof(ulong)];

    private static readonly UnsavedConfigFile _file = new("Proxies");
    private static readonly Dictionary<ConfigEntryBase, ConfigEntryBase> _proxyBySource = [];
    private static readonly Dictionary<ConfigEntryBase, (ConfigEntryBase Source, IDisposable Link)> _linkByProxy = [];

    public static ConfigEntryBase SourceOf(ConfigEntryBase proxy) => _linkByProxy.TryGetValue(proxy, out var link)
        ? link.Source
        : null;

    public static bool CanProxy(ConfigEntryBase source) => BinderFor(source) != null;

    public static ConfigEntryBase GetOrCreate(ConfigEntryBase source)
    {
        if (_proxyBySource.TryGetValue(source, out var existing))
            return existing;

        var bind = BinderFor(source);
        if (bind == null)
            return null;

        var proxy = bind(source);
        _proxyBySource[source] = proxy;
        return proxy;
    }

    public static void Discard(ConfigEntryBase source)
    {
        if (!_proxyBySource.TryGetValue(source, out var proxy))
            return;

        _linkByProxy[proxy].Link.Dispose();
        _linkByProxy.Remove(proxy);
        _proxyBySource.Remove(source);
        _file.Remove(proxy);
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
        return Bind(
            source,
            value => Convert.ToSingle(value, CultureInfo.InvariantCulture),
            number => Convert.ChangeType(number.ToString(CultureInfo.InvariantCulture), type, CultureInfo.InvariantCulture),
            NumericRange.Of(source)?.ToFloatRange());
    }

    private static ConfigEntryBase BindInt(ConfigEntryBase source)
    {
        var type = source.SettingType;
        return Bind(
            source,
            value => Convert.ToInt32(value, CultureInfo.InvariantCulture),
            number => Convert.ChangeType(number, type, CultureInfo.InvariantCulture),
            NumericRange.Of(source)?.ToIntRange());
    }

    private static ConfigEntryBase BindKeyCode(ConfigEntryBase source) => Bind(source, value => new KeyboardShortcut((KeyCode)value), shortcut => shortcut.MainKey, null);

    private static ConfigEntryBase BindText(ConfigEntryBase source)
    {
        var type = source.SettingType;
        return Bind(source, value => TomlTypeConverter.ConvertToString(value, type), text => TomlTypeConverter.ConvertToValue(text, type), null);
    }

    private static ConfigEntry<TProxy> Bind<TProxy>(ConfigEntryBase source, Func<object, TProxy> toProxy, Func<TProxy, object> toSource, AcceptableValueBase acceptableValues)
    {
        var description = new ConfigDescription(source.Description.Description, acceptableValues);
        var proxy = _file.Bind(toProxy(source.DefaultValue), description);
        var link = new ProxyLink<TProxy>(source, proxy, toProxy, toSource);

        try
        {
            link.Start();
        }
        catch
        {
            _file.Remove(proxy);
            throw;
        }

        _linkByProxy[proxy] = (source, link);
        return proxy;
    }
}
