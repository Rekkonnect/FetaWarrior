using Discord;
using Discord.Commands;
using FetaWarrior.DiscordFunctionality.Attributes;
using FetaWarrior.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    public class MessageHandlingModule : ModuleBase<SocketCommandContext>
    {
        #region Delete All
        [Command("delete all")]
        [Alias("remove all", "clear all")]
        [Summary("Deletes all messages that were sent in the channel that the command was sent in.")]
        [RequireGuildContext]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteAllMessages()
        {
            await DeleteAllMessages(Context.Channel as ITextChannel);
        }
        [Command("delete all")]
        [Alias("remove all", "clear all")]
        [Summary("Deletes all messages that were sent in the specified channel.")]
        [RequireGuildContext]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task DeleteAllMessages
        (
            [Summary("The channel ID of the channel whose all messages to delete.")]
            ITextChannel textChannel
        )
        {
            if (textChannel is not IGuildChannel guildChannel || guildChannel.GuildId != Context.Guild.Id)
            {
                await Context.Channel.SendMessageAsync("This server does not contain the provided channel.");
                return;
            }

            var firstMessageID = (await textChannel.GetFirstMessageAsync())?.Id ?? 0;
            var lastMessageID = (await textChannel.GetLastMessageAsync())?.Id ?? 0;

            if (firstMessageID == 0 || lastMessageID == 0)
            {
                await Context.Channel.SendMessageAsync("The channel contains no messages.");
                return;
            }

            await DeleteOtherChannelMessages(textChannel, firstMessageID, lastMessageID);
        }
        [Command("delete all")]
        [Alias("remove all", "clear all")]
        [Summary("Deletes all messages that were sent in the specified channel.")]
        [RequireGuildContext]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task DeleteAllMessages
        (
            [Summary("The channel ID of the channel whose all messages to delete.")]
            ulong channelID
        )
        {
            var channel = Context.Client.GetChannel(channelID);
            if (channel is null)
            {
                await Context.Channel.SendMessageAsync("This server does not contain the provided channel.");
                return;
            }
            if (channel is not ITextChannel textChannel)
            {
                await Context.Channel.SendMessageAsync("The provided channel is not a text channel.");
                return;
            }

            await DeleteAllMessages(textChannel);
        }
        #endregion

        #region Delete This Channel Messages
        [Command("delete here")]
        [Alias("remove here", "clear here")]
        [Summary("Deletes all messages from this channel that were sent after the provided message, **including** the first message. Only deletes messages in this channel.")]
        [RequireGuildContext]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteThisChannelMessages
        (
            [Summary("The ID of the first message that will be deleted, **inclusive**.")]
            ulong firstMessageID
        )
        {
            await DeleteOtherChannelMessages(Context.Channel as ITextChannel, firstMessageID);
        }
        [Command("delete here")]
        [Alias("remove here", "clear here")]
        [Summary("Deletes all messages from a specified channel that were sent within the provided message range, **including** the first and the last messages. Only deletes messages in that same channel.")]
        [RequireGuildContext]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteThisChannelMessages
        (
            [Summary("The ID of the first message that will be deleted, **inclusive**.")]
            ulong firstMessageID,
            [Summary("The ID of the last message that will be deleted, **inclusive**.")]
            ulong lastMessageID
        )
        {
            await DeleteOtherChannelMessages(Context.Channel as ITextChannel, firstMessageID, lastMessageID);
        }
        #endregion

        #region Delete Other Channel Messages
        [Command("delete oc")]
        [Alias("remove oc", "clear oc")]
        [Summary("Deletes all messages from a specified channel that were sent after the provided message, **including** the first message. Only deletes messages in that same channel.")]
        [RequireGuildContext]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteOtherChannelMessages
        (
            [Summary("The channel on which the the messages to delete are contained.")]
            ITextChannel textChannel,
            [Summary("The ID of the first message that will be deleted, **inclusive**.")]
            ulong firstMessageID
        )
        {
            var lastMessageID = (await textChannel.GetLastMessageAsync()).Id;
            await DeleteOtherChannelMessages(textChannel, firstMessageID, lastMessageID);
        }
        [Command("delete oc")]
        [Alias("remove oc", "clear oc")]
        [Summary("Deletes all messages from a specified channel that were sent within the provided message range, **including** the first and the last messages. Only deletes messages in that same channel.")]
        [RequireGuildContext]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteOtherChannelMessages
        (
            [Summary("The channel on which the the messages to delete are contained.")]
            ITextChannel textChannel,
            [Summary("The ID of the first message that will be deleted, **inclusive**.")]
            ulong firstMessageID,
            [Summary("The ID of the last message that will be deleted, **inclusive**.")]
            ulong lastMessageID
        )
        {
            var contextChannel = Context.Channel;

            if (firstMessageID == 0 || lastMessageID == 0)
            {
                await contextChannel.SendMessageAsync("The provided message IDs are invalid.");
                return;
            }

            var foundMessages = new HashSet<IMessage>();

            var originalProgressMessage = await contextChannel.SendMessageAsync($"Discovering messages to delete... 0 messages have been found so far.");
            var progressMessageTimestamp = originalProgressMessage.Timestamp;

            var persistentProgressMessage = new PersistentMessage(originalProgressMessage);

            for (ulong currentID = firstMessageID - 1; currentID < lastMessageID;)
            {
                var messages = await textChannel.GetMessagesAsync(currentID, Direction.After, 100).FlattenAsync();

                foreach (var message in messages)
                {
                    var id = message.Id;

                    if (id > currentID)
                        currentID = id;

                    if (id > lastMessageID)
                        continue;

                    foundMessages.Add(message);
                }

                await persistentProgressMessage.SetContentAsync($"Discovering messages to delete... {foundMessages.Count} messages have been found so far.");
            }

            // The progress message's timestamp is being used because it was used with Discord's clock
            // Avoiding clock difference issues
            var threshold = progressMessageTimestamp - TimeSpan.FromDays(14);
            foundMessages.Split(m => m.Timestamp.UtcDateTime < threshold, out var olderMessages, out var newerMessages);

            var newerMessageIDs = newerMessages.Select(m => m.Id).ToArray();

            int currentlyDeletedMessages = 0;
            bool deletionComplete = false;

            await persistentProgressMessage.SetContentAsync($"{foundMessages.Count} messages are being deleted...");
            await textChannel.DeleteMessagesAsync(newerMessageIDs);

            currentlyDeletedMessages += newerMessageIDs.Length;
            deletionComplete = foundMessages.Count == newerMessageIDs.Length;

            if (!deletionComplete)
            {
                var progressMessageUpdatingTask = UpdateProgressMessage();

                foreach (var message in olderMessages)
                {
                    await textChannel.DeleteMessageAsync(message);
                    currentlyDeletedMessages++;
                }

                deletionComplete = true;

                await progressMessageUpdatingTask;

                async Task UpdateProgressMessage()
                {
                    while (!deletionComplete)
                    {
                        var progressMessageContent = $"{foundMessages.Count} messages are being deleted... {currentlyDeletedMessages} messages have been deleted.";

                        await persistentProgressMessage.SetContentAsync(progressMessageContent);
                        await Task.Delay(1000);
                    }
                }
            }

            await persistentProgressMessage.SetContentAsync($"{foundMessages.Count} messages have been deleted.");
            await Task.Delay(5000);
            await persistentProgressMessage.DeleteAsync();
        }
        #endregion
    }
}
