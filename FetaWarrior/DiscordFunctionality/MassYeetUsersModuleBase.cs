using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.Rest;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    public abstract class MassYeetUsersModuleBase : ModuleBase<SocketCommandContext>
    {
        protected abstract string YeetAction { get; }
        protected abstract string YeetActionPastParticiple { get; }

        #region Server Messages
        protected async Task MassYeetFromServerMessages(ulong firstMessageID)
        {
            var messages = await Context.Channel.GetMessagesAsync(1).FlattenAsync();
            var lastID = messages.First().Id;
            await MassYeetFromServerMessages(firstMessageID, lastID);
        }
        protected async Task MassYeetFromServerMessages(ulong firstMessageID, ulong lastMessageID)
        {
            var guild = Context.Guild;
            var channel = guild.SystemChannel;

            var toYeet = new HashSet<ulong>();

            var progressMessage = await Context.Channel.SendMessageAsync($"Discovering users to {YeetAction}... 0 users have been found so far.");

            // firstID - 1 because the message retriever method retrieves messages after the given message's ID, excluding the original one
            for (ulong currentID = firstMessageID - 1; currentID < lastMessageID;)
            {
                var messages = await channel.GetMessagesAsync(currentID, Direction.After, 100).FlattenAsync();

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

                    toYeet.Add(s.Author.Id);
                }

                await progressMessage.ModifyAsync(m => m.Content = $"Discovering users to {YeetAction}... {toYeet.Count} users have been found so far.");
            }

            await MassYeetWithProgress(toYeet, progressMessage);
        }
        #endregion
        #region Join Date
        public async Task MassYeetFromJoinDate(ulong firstUserID)
        {
            var guild = Context.Guild;

            // Ensure that all users are downloaded
            await guild.DownloadUsersAsync();

            var lastJoinedUser = guild.Users.First();
            foreach (var user in guild.Users)
                if (user.JoinedAt > lastJoinedUser.JoinedAt)
                    lastJoinedUser = user;

            await MassYeetFromJoinDate(firstUserID, lastJoinedUser.Id);
        }
        public async Task MassYeetFromJoinDate(ulong firstUserID, ulong lastUserID)
        {
            var restGuild = await BotClientManager.Instance.RestClient.GetGuildAsync(Context.Guild.Id);

            var progressMessage = await Context.Channel.SendMessageAsync("Retrieving guild member list...");

            var users = await restGuild.GetUsersAsync().FlattenAsync();

            await progressMessage.ModifyAsync(m => m.Content = $"Discovering users to {YeetAction}...");

            var firstUser = users.First(u => u.Id == firstUserID);
            var lastUser = users.First(u => u.Id == lastUserID);

            var toBan = users.Where(u => u.JoinedAt >= firstUser.JoinedAt && u.JoinedAt <= lastUser.JoinedAt).Select(u => u.Id).ToArray();

            await MassYeetWithProgress(toBan, progressMessage);
        }
        #endregion

        protected abstract Task YeetUser(ulong userID, string reason);

        private async Task MassYeetWithProgress(ICollection<ulong> toYeet, RestUserMessage progressMessage)
        {
            int yeetedUserCount = 0;
            int forbiddenOperationCount = 0;
            bool yeetingComplete = false;
            var progressUpdatingTask = UpdateYeetingProgress();

            foreach (ulong userID in toYeet)
            {
                bool success = false;
                while (!success)
                {
                    success = true;
                    try
                    {
                        await YeetUser(userID, $"Mass {YeetActionPastParticiple}");
                    }
                    catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
                    {
                        forbiddenOperationCount++;
                    }
                    catch
                    {
                        // In case of any other error, attempt to retry
                        success = false;
                    }
                }
                yeetedUserCount++;
            }

            yeetingComplete = true;
            await progressUpdatingTask;

            async Task UpdateYeetingProgress()
            {
                while (!yeetingComplete)
                {
                    await progressMessage.ModifyAsync(m => m.Content = $"{toYeet.Count} users are being {YeetActionPastParticiple}... {yeetedUserCount} users have been {YeetActionPastParticiple} so far.");
                    await Task.Delay(1000);
                }

                var finalizedMessage = $"{toYeet.Count} users have been {YeetActionPastParticiple}.";
                if (forbiddenOperationCount > 0)
                    finalizedMessage += $"\n{forbiddenOperationCount} users could not be {YeetActionPastParticiple}.";

                await progressMessage.ModifyAsync(m => m.Content = finalizedMessage);
            }
        }
    }
}
