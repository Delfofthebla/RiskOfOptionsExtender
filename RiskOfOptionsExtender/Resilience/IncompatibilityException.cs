using System;

namespace RiskOfOptionsExtender.Resilience;

internal sealed class IncompatibilityException(string message) : Exception(message)
{
    public static bool Indicates(Exception exception) => exception is IncompatibilityException or MissingMemberException or TypeLoadException;
}
