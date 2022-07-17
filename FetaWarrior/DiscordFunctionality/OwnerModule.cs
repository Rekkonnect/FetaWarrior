using Discord;
using Discord.Commands;
using Discord.Net;
using FetaWarrior.Extensions;
using Garyon.DataStructures;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[RequireOwner]
public class OwnerModule : SocketModule
{
    #region Guilds
    [Command("guilds")]
    public async Task GetGuildsAsync()
    {
        await GetGuildsAsync(false);
    }
    [Command("guilds o")]
    public async Task GetGuildsWithOwnersAsync()
    {
        await GetGuildsAsync(true);
    }

    private async Task GetGuildsAsync(bool includeOwners)
    {
        var typingState = Context.Channel.EnterTypingState();

        var guilds = BotClientManager.Instance.Client.Guilds;
        var builder = new StringBuilder($"{guilds.Count} Guilds\n\n");
        foreach (var g in guilds)
        {
            builder.Append($"{g.Name} (ID: {g.Id}) - {g.MemberCount} Members");

            if (includeOwners)
            {
                var restGuild = await BotClientManager.Instance.RestClient.GetGuildAsync(g.Id);
                var restOwner = await restGuild.GetUserAsync(g.OwnerId);
                builder.Append($" - Owner: {restOwner.Username}#{restOwner.Discriminator}");
            }

            builder.AppendLine();
        }

        await Context.Channel.SendMessageAsync(builder.ToString());

        typingState.Dispose();
    }
    #endregion

    #region Bot Updates
    [Command("update owners unban")]
    public async Task UpdateOwnersUnbanDeletedAccounts()
    {
        var ownerDictionary = new FlexibleInitializableValueDictionary<ulong, HashSet<ulong>>();
        var guilds = await BotClientManager.Instance.RestClient.GetGuildsAsync();

        foreach (var guild in guilds)
        {
            try
            {
                var bans = await guild.GetAllBansAsync();

                // Avoid sending this to guilds with low ban counts, since they are not cluttered enough to make good use of the feature
                if (bans.Count < 50)
                    continue;
            }
            catch (HttpException ex) when (ex.DiscordCode is not null) { }

            ownerDictionary[guild.OwnerId].Add(guild.Id);
        }

        foreach (var owner in ownerDictionary)
        {
            var targetUser = await guilds.First(guild => guild.Id == owner.Value.First()).GetOwnerAsync();

            try
            {
                await targetUser.SendMessageAsync($@"
Greetings,

as the owner of server(s) this bot is in, the developer has decided to inform you that there now is a new feature you could probably benefit from, which revokes the ban for all accounts that have been detected as deleted, reducing the clutter in the banned users section in the server settings. The command is called `unban deleted`. It is detected that you could benefit from this command on the following servers you own:

{string.Join('\n', owner.Value.Select(ownedGuild => guilds.First(guild => guild.Id == ownedGuild)))}

Please note the following:

- the way deleted accounts are detected is not 100% reliable, as the API does not currently have the ability to send that information -- possible false positives
- this command requires granting the bot the Manage Server permission.");
            }
            catch { }
        }
    }

    [Command("update owners da")]
    public async Task UpdateOwnersDeleteSourceDeletedCrosspostMessages()
    {
        var ownerIDs = new HashSet<ulong>();
        var guilds = await BotClientManager.Instance.RestClient.GetGuildsAsync();

        foreach (var guild in guilds)
        {
            ownerIDs.Add(guild.OwnerId);
        }

        foreach (var ownerID in ownerIDs)
        {
            var targetUser = await BotClientManager.Instance.RestClient.GetUserAsync(ownerID);

            try
            {
                await targetUser.SendMessageAsync(@"
Greetings,

as the owner of server(s) this bot is in, the developer has decided to inform you that there now is a new feature you could probably benefit from, which automatically removes all crosspost messages whose source message is originally deleted (they appear as ""[Original Message Deleted]""). The related commands are called `delete da`, `delete oc da`, `delete all da` and `delete server da`. You can always use the help command to find out how these commands work.

As a heads-up, those commands will take a load of time to execute, meaning they could have their execution interrupted during the bot's downtime. If no progress is being made within 5 minutes after the bot becomes online, you can retry the command to guarantee continuation of the execution.
");
            }
            catch { }
        }
    }
    #endregion

    #region Messages
    [Command("warndev")]
    public async Task SendDevelopmentWarning(ulong userID)
    {
        await SendMessage(userID, $"Greetings,\n\nYou are being sent this message because the developer wanted to inform you that the bot is being development **at the moment**, so please do not run any commands that could break at any moment due to restarting the client. Reach the developer out by sending him a friend request at: `Rekkon#2783`.");
    }
    [Command("informready")]
    public async Task SendReadyDeployment(ulong userID)
    {
        await SendMessage(userID, $"Greetings,\n\nThe bot is currently available for consumption. For any further inconveniences and development breaks, you will be informed beforehand. Sorry for any inconveniences that might've been caused. Reach the developer out by sending him a friend request at: `Rekkon#2783`.");
    }
    [Command("send")]
    public async Task SendMessage(ulong userID, [Remainder] string message)
    {
        var restUser = await BotClientManager.Instance.RestClient.GetUserAsync(userID);
        await restUser.SendMessageAsync(message);
    }
    #endregion
}
