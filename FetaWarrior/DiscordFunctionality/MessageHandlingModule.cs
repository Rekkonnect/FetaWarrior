using Discord;
using Discord.Commands;
using FetaWarrior.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    public class MessageHandlingModule : ModuleBase<SocketCommandContext>
    {
        [Command("delete all")]
        [Alias("remove all", "clear all")]
        [Summary("Deletes all messages that were sent in the channel that the command was sent in.")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteAllMessages()
        {
            var firstMessageID = (await Context.Channel.GetFirstMessageAsync()).Id;
            var lastMessageID = (await Context.Channel.GetLastMessageAsync()).Id;
            await DeleteMessages(firstMessageID, lastMessageID);
        }

        [Command("delete")]
        [Alias("remove", "clear")]
        [Summary("Deletes all messages after the provided message, **including** the first message. Only deletes messages in that same channel.")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteMessages
        (
            [Summary("The ID of the first message that will be deleted, **inclusive**.")]
            ulong firstMessageID
        )
        {
            var lastMessageID = (await Context.Channel.GetLastMessageAsync()).Id;
            await DeleteMessages(firstMessageID, lastMessageID);
        }
        [Command("delete")]
        [Alias("remove", "clear")]
        [Summary("Deletes all messages within the provided message range, **including** the first and the last messages. Only deletes messages in that same channel.")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteMessages
        (
            [Summary("The ID of the first message that will be deleted, **inclusive**.")]
            ulong firstMessageID,
            [Summary("The ID of the last message that will be deleted, **inclusive**.")]
            ulong lastMessageID
        )
        {
            var channel = Context.Channel;

            var toDelete = new HashSet<ulong>();

            var progressMessage = await channel.SendMessageAsync($"Discovering messages to delete... 0 messages have been found so far.");

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

                    toDelete.Add(id);
                }

                await progressMessage.ModifyAsync(m => m.Content = $"Discovering messages to delete... {toDelete.Count} messages have been found so far.");
            }

            if (Context.Channel is ITextChannel textChannel)
            {
                await progressMessage.ModifyAsync(m => m.Content = $"{toDelete.Count} messages are being deleted...");
                await textChannel.DeleteMessagesAsync(toDelete);
                await progressMessage.ModifyAsync(m => m.Content = $"{toDelete.Count} messages have been deleted.");
                await Task.Delay(5000);
                await progressMessage.DeleteAsync();
                return;
            }

            // Support non-guild text channels
            int deletedMessageCount = 0;
            bool deletingComplete = false;
            var progressUpdatingTask = UpdateDeletionProgress();

            foreach (ulong messageID in toDelete)
            {
                // Awaiting because unknown issues that could be caused wihout awaiting
                await channel.DeleteMessageAsync(messageID);
                deletedMessageCount++;
            }

            deletingComplete = true;
            await progressUpdatingTask;

            // Delete the progress message after 5 seconds
            await Task.Delay(5000);
            await progressMessage.DeleteAsync();

            async Task UpdateDeletionProgress()
            {
                while (!deletingComplete)
                {
                    await progressMessage.ModifyAsync(m => m.Content = $"{toDelete.Count} messages are being deleted... {deletedMessageCount} messages have been deleted so far.");
                    await Task.Delay(1000);
                }
                await progressMessage.ModifyAsync(m => m.Content = $"{toDelete.Count} messages have been deleted.");
            }
        }
    }
}
