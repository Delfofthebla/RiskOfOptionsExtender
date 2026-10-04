using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace RiskOfOptionsExtender.UI;

internal sealed class HookSet(params (MethodBase Target, Delegate Detour)[] detours)
{
    private readonly List<Hook> _hooks = [];

    public bool IsInstalled => _hooks.Count > 0;

    public void SetInstalled(bool installed)
    {
        if (installed)
            Install();
        else
            Uninstall();
    }

    private void Install()
    {
        if (IsInstalled)
            return;

        foreach (var (target, detour) in detours)
            _hooks.Add(new Hook(target, detour));
    }

    private void Uninstall()
    {
        foreach (var hook in _hooks)
            hook.Dispose();

        _hooks.Clear();
    }
}
