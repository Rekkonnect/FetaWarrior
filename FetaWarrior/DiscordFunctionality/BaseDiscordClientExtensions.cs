using Discord.Rest;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public static class BaseDiscordClientExtensions
{
    public static async Task LoginAsync(this BaseDiscordClient discordClient, BotCredentials credentials)
    {
        await discordClient.LoginAsync(credentials.TokenType, credentials.BotToken);
    }
}
