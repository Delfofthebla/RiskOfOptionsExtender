using System;
using System.Globalization;
using BepInEx.Configuration;

namespace RiskOfOptionsExtender.Generation;

internal readonly record struct NumericRange(decimal Min, decimal Max)
{
    public bool FitsInInt => Min >= int.MinValue && Max <= int.MaxValue;

    public static NumericRange? Of(ConfigEntryBase entry)
    {
        var acceptableValues = entry.Description.AcceptableValues;
        var type = acceptableValues?.GetType();
        if (type == null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(AcceptableValueRange<>))
            return null;

        try
        {
            var min = Convert.ToDecimal(type.GetProperty("MinValue").GetValue(acceptableValues), CultureInfo.InvariantCulture);
            var max = Convert.ToDecimal(type.GetProperty("MaxValue").GetValue(acceptableValues), CultureInfo.InvariantCulture);
            return min < max
                ? new(min, max)
                : null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    public AcceptableValueRange<float> ToFloatRange() => new((float)Min, (float)Max);

    public AcceptableValueRange<int> ToIntRange() => new((int)Min, (int)Max);
}
