namespace FetaWarrior.Extensions;

public static class DiscordTextExtensions
{
    private static readonly char[] formatters = { '\\', '|', '*', '`', '_' };

    public static string ToNonFormattableText(this string input)
    {
        var result = input;
        foreach (var f in formatters)
            result = result.Replace($@"{f}", $@"\{f}");
        return result;
    }
}
