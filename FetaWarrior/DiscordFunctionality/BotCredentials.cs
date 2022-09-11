using Discord;
using Discord.Net.Rest;
using static System.IO.File;

namespace FetaWarrior.DiscordFunctionality;

public class BotCredentials
{
    public static BotCredentials Instance { get; }

    static BotCredentials()
    {
        // NEVER publish secret things like these (keep them at a secret file no one reads shhh)
        Instance = ReadFromFileLines(ReadAllLines("secrets.txt"));
    }

    public ulong ClientID { get; set; }
    public string ClientSecret { get; set; }
    public string BotToken { get; set; }

    public TokenType TokenType { get; set; } = TokenType.Bot;

    private static BotCredentials ReadFromFileLines(string[] lines)
    {
        return new()
        {
            ClientID = ulong.Parse(lines[0]),
            ClientSecret = lines[1],
            BotToken = lines[2],
        };
    }
}
