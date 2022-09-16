using Discord;
using Discord.Interactions;
using Discord.Net;
using FetaWarrior.DiscordFunctionality.Interactions.Attributes;
using FetaWarrior.Extensions;
using Garyon.DataStructures;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[RequireDMContext]
[RequireOwner]
public class OwnerModule : SocketInteractionModule
{
    #region Guilds
    [SlashCommand("guilds", "Gets all the guilds that this bot is in. Useful to invoke when this bot is not in too many servers.")]
    public async Task GetGuildsAsync()
    {
        await GetGuildsAsync(false);
    }
    [SlashCommand("guilds-o", "Gets all the guilds that this bot is in, including their owners, which is ***s l o w***.")]
    public async Task GetGuildsWithOwnersAsync()
    {
        await GetGuildsAsync(true);
    }

    private async Task GetGuildsAsync(bool includeOwners)
    {
        var deferral = Context.Interaction.DeferAsync();

        var guilds = BotClientManager.Instance.Client.Guilds
            .OrderBy(g => g.Id).ToArray();

        var builder = new StringBuilder($"{guilds.Length} Guilds\n\n");
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

        await deferral;
        await Context.Interaction.UpdateResponseTextAsync(builder.ToString());
    }
    #endregion

    #region Bot Updates
    [Group("update-owners", "Contains commands that update the owners of guilds about a new feature the bot includes.")]
    public class UpdateOwners
    {
        [SlashCommand("unban", "Informs the owners of the servers that the bot can now unban deleted accounts.")]
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

as the owner of server(s) this bot is in, the developer has decided to inform you that there now is a new feature you could probably benefit from, which revokes the ban for all accounts that have been detected as deleted, reducing the clutter in the banned users section in the server settings. The command is called `unban-deleted`. It is detected that you could benefit from this command on the following servers you own:

{string.Join('\n', owner.Value.Select(ownedGuild => guilds.First(guild => guild.Id == ownedGuild)))}

Please note the following:

- the way deleted accounts are detected is not 100% reliable, as the API does not currently have the ability to send that information -- possible false positives
- this command requires granting the bot the Manage Server permission.");
                }
                catch { }
            }
        }

        [SlashCommand("removed-an", "Informs the owners of the servers about the removed-announcements comamnds.")]
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

as the owner of server(s) this bot is in, the developer has decided to inform you that there now is a new feature you could probably benefit from, which automatically removes all crosspost messages whose source message is originally deleted (they appear as ""[Original Message Deleted]""). The related commands belong in the `delete removed-an` subgroup. You can always use the help command to find out how these commands work.

As a heads-up, those commands will take a load of time to execute, meaning they could have their execution interrupted during the bot's downtime. If no progress is being made within 5 minutes after the bot becomes online, you can retry the command to guarantee continuation of the execution.
");
                }
                catch { }
            }
        }
    }
    #endregion

    #region Messages
    [SlashCommand("warndev", "Sends a message to the specified user, notifying them that the bot is currently in development.")]
    public async Task SendDevelopmentWarning(Snowflake userID)
    {
        await SendMessage(userID, $"Greetings,\n\nYou are being sent this message because the developer wanted to inform you that the bot is being development **at the moment**, so please do not run any commands that could break at any moment due to restarting the client. Reach the developer out by sending him a friend request at: `Rekkon#2783`.");
    }
    [SlashCommand("informready", "Sends a message to the specified user, informing them that the bot is now ready to use.")]
    public async Task SendReadyDeployment(Snowflake userID)
    {
        await SendMessage(userID, $"Greetings,\n\nThe bot is currently available for consumption. For any further inconveniences and development breaks, you will be informed beforehand. Sorry for any inconveniences that might've been caused. Reach the developer out by sending him a friend request at: `Rekkon#2783`.");
    }
    [SlashCommand("send", "Sends a message to somebody.")]
    public async Task SendMessage(Snowflake userID, string message)
    {
        var restUser = await BotClientManager.Instance.RestClient.GetUserAsync(userID);
        if (restUser is null)
        {
            await Context.Channel.SendMessageAsync("The selected user ID was not found. Please make sure that the user exists and shares a server with me!");
        }
        await restUser.SendMessageAsync(message);
    }
    #endregion
}
