using System;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Generation;

internal sealed class ProxyLink<TProxy>(ConfigEntryBase source, ConfigEntry<TProxy> proxy, Func<object, TProxy> toProxy, Func<TProxy, object> toSource)
{
    private bool _syncing;

    public void Start()
    {
        PullFromSource();
        proxy.SettingChanged += OnProxyChanged;
        source.ConfigFile.SettingChanged += OnSourceFileChanged;
    }

    private void OnProxyChanged(object sender, EventArgs args)
    {
        if (!_syncing)
            Sync(PushToSource);
    }

    private void OnSourceFileChanged(object sender, SettingChangedEventArgs args)
    {
        if (!_syncing && args.ChangedSetting == source)
            Sync(PullFromSource);
    }

    private void PushToSource()
    {
        try
        {
            source.BoxedValue = toSource(proxy.Value);
        }
        catch (Exception exception)
        {
            RiskOfOptionsExtenderPlugin.Log.LogWarning($"\"{proxy.Value}\" is not a valid value for {source.Definition}: {exception.Message}");
        }

        PullFromSource();
    }

    private void PullFromSource()
    {
        proxy.Value = toProxy(source.BoxedValue);
    }

    private void Sync(Action sync)
    {
        _syncing = true;
        try
        {
            sync();
        }
        finally
        {
            _syncing = false;
        }
    }
}
