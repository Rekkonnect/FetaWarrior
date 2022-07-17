using Discord;
using Discord.Commands;
using FetaWarrior.DiscordFunctionality.Attributes;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[Group("massban")]
[Summary("Mass bans all users that suit a specified filter.")]
[RequireGuildContext]
[RequireUserPermission(GuildPermission.BanMembers)]
[RequireBotPermission(GuildPermission.BanMembers)]
public class MassBanModule : MassYeetUsersModuleBase
{
    public override UserYeetingLexemes Lexemes => new MassBanningLexemes();

    #region Server Messages
    [Command("server message")]
    [Alias("sm", "server messages", "servermessage", "servermessages")]
    [Summary("Mass bans all users that are greeted with server messages after the given server message. This means that all server messages that greet new members after the specified message, **including** the specified message, will result in the greeted members' **ban**.")]
    public async Task MassBanFromServerMessages
    (
        [Summary("The ID of the first server message, inclusive.")]
        ulong firstMessageID
    )
    {
        await MassYeetFromServerMessages(firstMessageID);
    }
    [Command("server message")]
    [Alias("sm", "server messages", "servermessage", "servermessages")]
    [Summary("Mass bans all users that are greeted with server messages within the given server message range. This means that all server messages that greet new members within the specified range, **including** the specified messages, will result in the greeted members' **ban**.")]
    public async Task MassBanFromServerMessages
    (
        [Summary("The ID of the first server message, inclusive.")]
        ulong firstMessageID,
        [Summary("The ID of the last server message, inclusive.")]
        ulong lastMessageID
    )
    {
        await MassYeetFromServerMessages(firstMessageID, lastMessageID);
    }
    #endregion
    #region Join Date
    [Command("join date")]
    [Alias("jd", "joindate")]
    [Summary("Mass bans all users that joined after a user's join date. This means that all users that joined after the first specified user, **including** the user that was specified as first, will be **banned**.")]
    public async Task MassBanFromJoinDate
    (
        [Summary("The ID of the first user, inclusive.")]
        ulong firstUserID
    )
    {
        await MassYeetFromJoinDate(firstUserID);
    }
    [Command("join date")]
    [Alias("jd", "joindate")]
    [Summary("Mass bans all users that joined within the range specified by two users' join dates. This means that all users that joined after the first specified user, and before the last specified user, **including** the users that were specified as first and last, will be **banned**.")]
    public async Task MassBanFromJoinDate
    (
        [Summary("The ID of the first user, inclusive.")]
        ulong firstUserID,
        [Summary("The ID of the last user, inclusive.")]
        ulong lastUserID
    )
    {
        await MassYeetFromJoinDate(firstUserID, lastUserID);
    }
    #endregion

    protected override async Task YeetUser(ulong userID, string reason) => await Context.Guild.AddBanAsync(userID, 7, reason);

    private sealed class MassBanningLexemes : UserYeetingLexemes
    {
        public override string ActionName => "ban";
        public override string ActionPastParticiple => "banned";
    }
}
