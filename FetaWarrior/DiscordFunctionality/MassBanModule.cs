using Discord;
using Discord.Commands;
using Discord.Rest;
using FetaWarrior.DiscordFunctionality.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    [Group("massban")]
    [Summary("Massbans all users that suit a specified filter.")]
    public class MassBanModule : ModuleBase<SocketCommandContext>
    {
        #region Server Messages
        [Command("server message")]
        [Summary("Mass bans all users that are greeted with server messages after the given server message. This means that all server messages that greet new members after the specified message, **including** the specified message, will result in the greeted members' **ban**.")]
        [Alias("sm", "server messages", "servermessage", "servermessages")]
        [RequireContext(ContextType.Guild)]
        [RequireAdminPermission]
        public async Task MassBanFromServerMessages
        (
            [Name("firstMessageID")]
            [Summary("The ID of the first server message, inclusive.")]
            ulong firstMessageID
        )
        {
            var messages = await Context.Channel.GetMessagesAsync(1).FlattenAsync();
            var lastID = messages.First().Id;
            await MassBanFromServerMessages(firstMessageID, lastID);
        }
        [Command("server message")]
        [Summary("Mass bans all users that are greeted with server messages within the given server message range. This means that all server messages that greet new members within the specified range, **including** the specified messages, will result in the greeted members' **ban**.")]
        [Alias("sm", "server messages", "servermessage", "servermessages")]
        [RequireContext(ContextType.Guild)]
        [RequireAdminPermission]
        public async Task MassBanFromServerMessages
        (
            [Name("firstMessageID")]
            [Summary("The ID of the first server message, inclusive.")]
            ulong firstMessageID,
            [Name("lastMessageID")]
            [Summary("The ID of the last server message, inclusive.")]
            ulong lastMessageID
        )
        {
            var guild = Context.Guild;
            var channel = guild.SystemChannel;

            var toBan = new HashSet<ulong>();

            var progressMessage = await Context.Channel.SendMessageAsync("Discovering users to ban... 0 users have been found so far.");

            // firstID - 1 because the message retriever method retrieves messages after the given message's ID, excluding the original one
            for (ulong currentID = firstMessageID - 1; currentID < lastMessageID; )
            {
                var messages = await channel.GetMessagesAsync(currentID, Direction.After, 100).FlattenAsync();

                // Reduce the awaitage to the flattened
                foreach (var message in messages)
                {
                    var id = message.Id;

                    // Set the current ID to the maximum found ID
                    if (id > currentID)
                        currentID = id;

                    if (id > lastMessageID)
                        continue;

                    if (message is not RestSystemMessage s)
                        continue;

                    if (s.Type != MessageType.GuildMemberJoin)
                        continue;

                    toBan.Add(s.Author.Id);
                }

                await progressMessage.ModifyAsync(m => m.Content = $"Discovering users to ban... {toBan.Count} users have been found so far.");
            }

            await MassBanWithProgress(toBan, progressMessage);
        }
        #endregion
        #region Join Date
        [Command("join date")]
        [Summary("Mass bans all users that joined after a user's join date. This means that all users that joined after the first specified user, **including** the user that was specified as first, will be **banned**.")]
        [Alias("jd", "joindate")]
        [RequireContext(ContextType.Guild)]
        [RequireAdminPermission]
        public async Task MassBanFromJoinDate
        (
            [Name("firstUserID")]
            [Summary("The ID of the first user, inclusive.")]
            ulong firstUserID
        )
        {
            var guild = Context.Guild;

            // Ensure that all users are downloaded
            await guild.DownloadUsersAsync();

            var lastJoinedUser = guild.Users.First();
            foreach (var user in guild.Users)
                if (user.JoinedAt > lastJoinedUser.JoinedAt)
                    lastJoinedUser = user;
            
            await MassBanFromJoinDate(firstUserID, lastJoinedUser.Id);
        }
        [Command("join date")]
        [Summary("Mass bans all users that joined within the range specified by two users' join dates. This means that all users that joined after the first specified user, and before the last specified user, **including** the users that were specified as first and last, will be **banned**.")]
        [Alias("jd", "joindate")]
        [RequireContext(ContextType.Guild)]
        [RequireAdminPermission]
        public async Task MassBanFromJoinDate
        (
            [Name("firstUserID")]
            [Summary("The ID of the first user, inclusive.")]
            ulong firstUserID,
            [Name("lastUserID")]
            [Summary("The ID of the last user, inclusive.")]
            ulong lastUserID
        )
        {
            var guild = Context.Guild;

            var progressMessage = await Context.Channel.SendMessageAsync("Updating guild member list...");

            // Ensure that all users are downloaded
            await guild.DownloadUsersAsync();

            await progressMessage.ModifyAsync(m => m.Content = $"Discovering users to ban...");

            var firstUser = guild.GetUser(firstUserID);
            var lastUser = guild.GetUser(lastUserID);

            var toBan = guild.Users.Where(u => u.JoinedAt >= firstUser.JoinedAt && u.JoinedAt <= lastUser.JoinedAt).Select(u => u.Id).ToArray();

            await MassBanWithProgress(toBan, progressMessage);
        }
        #endregion

        private async Task MassBanWithProgress(ICollection<ulong> toBan, RestUserMessage progressMessage)
        {
            int bannedUserCount = 0;
            bool banningComplete = false;
            var progressUpdatingTask = UpdateBanningProgress();

            foreach (ulong userID in toBan)
            {
                // Awaiting because unknown issues that could be caused wihout awaiting
                await Context.Guild.AddBanAsync(userID, 7, "Massbanned");
                bannedUserCount++;
            }

            banningComplete = true;
            await progressUpdatingTask;

            async Task UpdateBanningProgress()
            {
                while (!banningComplete)
                {
                    await progressMessage.ModifyAsync(m => m.Content = $"{toBan.Count} users are being banned... {bannedUserCount} users have been banned so far.");
                    await Task.Delay(1000);
                }
                await progressMessage.ModifyAsync(m => m.Content = $"{toBan.Count} users have been banned.");
            }
        }
    }
}
