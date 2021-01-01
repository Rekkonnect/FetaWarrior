using Discord;
using Discord.Commands;
using FetaWarrior.DiscordFunctionality.Attributes;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    [Group("masskick")]
    [Summary("Mass kicks all users that suit a specified filter.")]
    public class MassKickModule : MassYeetUsersModuleBase
    {
        protected override string YeetAction => "kick";
        protected override string YeetActionPastParticiple => "kicked";

        #region Server Messages
        [Command("server message")]
        [Summary("Mass kicks all users that are greeted with server messages after the given server message. This means that all server messages that greet new members after the specified message, **including** the specified message, will result in the greeted members' **kick**.")]
        [Alias("sm", "server messages", "servermessage", "servermessages")]
        [RequireGuildContext]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task MassKickFromServerMessages
        (
            [Name("firstMessageID")]
            [Summary("The ID of the first server message, inclusive.")]
            ulong firstMessageID
        )
        {
            await MassYeetFromServerMessages(firstMessageID);
        }
        [Command("server message")]
        [Summary("Mass kicks all users that are greeted with server messages within the given server message range. This means that all server messages that greet new members within the specified range, **including** the specified messages, will result in the greeted members' **kick**.")]
        [Alias("sm", "server messages", "servermessage", "servermessages")]
        [RequireGuildContext]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task MassKickFromServerMessages
        (
            [Name("firstMessageID")]
            [Summary("The ID of the first server message, inclusive.")]
            ulong firstMessageID,
            [Name("lastMessageID")]
            [Summary("The ID of the last server message, inclusive.")]
            ulong lastMessageID
        )
        {
            await MassYeetFromServerMessages(firstMessageID, lastMessageID);
        }
        #endregion
        #region Join Date
        [Command("join date")]
        [Summary("Mass kicks all users that joined after a user's join date. This means that all users that joined after the first specified user, **including** the user that was specified as first, will be **kicked**.")]
        [Alias("jd", "joindate")]
        [RequireGuildContext]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task MassKickFromJoinDate
        (
            [Name("firstUserID")]
            [Summary("The ID of the first user, inclusive.")]
            ulong firstUserID
        )
        {
            await MassYeetFromJoinDate(firstUserID);
        }
        [Command("join date")]
        [Summary("Mass kicks all users that joined within the range specified by two users' join dates. This means that all users that joined after the first specified user, and before the last specified user, **including** the users that were specified as first and last, will be **kicked**.")]
        [Alias("jd", "joindate")]
        [RequireGuildContext]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task MassKickFromJoinDate
        (
            [Name("firstUserID")]
            [Summary("The ID of the first user, inclusive.")]
            ulong firstUserID,
            [Name("lastUserID")]
            [Summary("The ID of the last user, inclusive.")]
            ulong lastUserID
        )
        {
            await MassYeetFromJoinDate(firstUserID, lastUserID);
        }
        #endregion

        protected override async Task YeetUser(ulong userID, string reason)
        {
            var guild = await BotClientManager.Instance.RestClient.GetGuildAsync(Context.Guild.Id);
            var user = await guild.GetUserAsync(userID);
            await user.KickAsync(reason);
        }
    }
}
