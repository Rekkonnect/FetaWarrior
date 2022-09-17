using Discord;

namespace FetaWarrior.Extensions;

public static class ApplicationCommandOptionTypeExtensions
{
    public static bool IsSubCommand(this ApplicationCommandOptionType type)
    {
        return type is ApplicationCommandOptionType.SubCommandGroup or ApplicationCommandOptionType.SubCommand;
    }
}
