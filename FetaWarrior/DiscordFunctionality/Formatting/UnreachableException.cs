using System;

namespace FetaWarrior.DiscordFunctionality.Formatting;

public sealed class UnreachableException : Exception
{
    public UnreachableException()
        : this("This code point is unreachable") { }

    public UnreachableException(string message)
        : base(message) { }

    public static void Throw() => throw new UnreachableException();
    public static void Throw(string message) => throw new UnreachableException(message);
}