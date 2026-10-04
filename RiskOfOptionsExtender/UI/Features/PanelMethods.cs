using System;
using System.Reflection;
using RiskOfOptions.Components.Panel;
using RiskOfOptionsExtender.Resilience;

namespace RiskOfOptionsExtender.UI.Features;

internal static class PanelMethods
{
    private const BindingFlags AnyInstanceMethod = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public static MethodInfo Start => Find(nameof(ModOptionPanelController.Start));

    public static MethodInfo LoadModOptions => Find(nameof(ModOptionPanelController.LoadModOptionsFromOptionCollection), typeof(string));

    public static MethodInfo LoadCategory => Find(nameof(ModOptionPanelController.LoadOptionListFromCategory), typeof(string), typeof(int));

    private static MethodInfo Find(string name, params Type[] parameterTypes)
    {
        return typeof(ModOptionPanelController).GetMethod(name, AnyInstanceMethod, null, parameterTypes, null)
            ?? throw new IncompatibilityException($"{nameof(ModOptionPanelController)}.{name}({string.Join<Type>(", ", parameterTypes)}) no longer exists.");
    }
}
