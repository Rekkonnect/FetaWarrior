namespace FetaWarrior.Extensions
{
    public static class DiscordTextExtensions
    {
        public static char[] Formatters = { '\\', '|', '*', '`', '_' };

        public static string ToNonFormattableText(this string input)
        {
            var result = input;
            foreach (var f in Formatters)
                result = result.Replace($@"{f}", $@"\{f}");
            return result;
        }
    }
}
