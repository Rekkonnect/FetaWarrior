using Discord;
using Discord.Commands;
using System.Text;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    [RequireOwner]
    public class OwnerModule : ModuleBase<SocketCommandContext>
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
}
