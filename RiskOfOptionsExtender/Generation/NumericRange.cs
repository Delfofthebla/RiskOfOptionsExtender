using System;
using System.Globalization;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Generation;

internal sealed class NumericRange(decimal min, decimal max)
{
    public decimal Min { get; } = min;

    public decimal Max { get; } = max;

    public bool FitsInInt => Min >= int.MinValue && Max <= int.MaxValue;

    public static NumericRange Of(ConfigEntryBase entry)
    {
        var acceptableValues = entry.Description.AcceptableValues;
        var type = acceptableValues?.GetType();
        if (type == null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(AcceptableValueRange<>))
            return null;

        try
        {
            var min = Convert.ToDecimal(type.GetProperty("MinValue")!.GetValue(acceptableValues), CultureInfo.InvariantCulture);
            var max = Convert.ToDecimal(type.GetProperty("MaxValue")!.GetValue(acceptableValues), CultureInfo.InvariantCulture);
            return min < max ? new NumericRange(min, max) : null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }
}
