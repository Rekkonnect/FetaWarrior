using Discord;
using System.Linq;

namespace FetaWarrior.Extensions;

public static class IApplicationCommandInteractionDataExtensions
{
    public static string GetFullCommandName(this IApplicationCommandInteractionData data, out IApplicationCommandInteractionDataOption deepmostSubCommand)
    {
        var commandString = data.Name;
        var options = data.Options;
        deepmostSubCommand = null;
        while (true)
        {
            var next = options.FirstOrDefault();

            bool isSubcommand = next?.Type.IsSubCommand() is true;
            if (!isSubcommand)
                break;

            commandString += $" {next.Name}";
            options = next.Options;
            deepmostSubCommand = next;
        }
        return commandString;
    }
}
