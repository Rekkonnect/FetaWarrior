using Discord;
using FetaWarrior.DiscordFunctionality;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.Extensions;

public static class IMessageChannelExtensions
{
    private static readonly DateTime minValidDate = new(2000, 1, 1);

    public static async Task<IMessage> GetFirstMessageAsync(this IMessageChannel channel)
    {
        return (await channel.GetMessagesAsync(0, Direction.After, 1).FirstAsync()).FirstOrDefault();
    }
    public static async Task<IMessage> GetLastMessageAsync(this IMessageChannel channel)
    {
        return (await channel.GetMessagesAsync(1).FirstAsync()).FirstOrDefault();
    }

    public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID)
    {
        return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, null, null);
    }
    public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, Predicate<IMessage> messagePredicate)
    {
        return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, messagePredicate, null);
    }
    public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, Action<int> progressReporter)
    {
        return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, null, progressReporter);
    }
    public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, Func<int, Task> progressReporter)
    {
        return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, null, progressReporter);
    }
    public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, DateTime minTimestamp, Func<int, Task> progressReporter)
    {
        return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, minTimestamp, null, progressReporter);
    }
    public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, Predicate<IMessage> messagePredicate, Action<int> progressReporter)
    {
        return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, minValidDate, messagePredicate, progressReporter);
    }
    public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, Predicate<IMessage> messagePredicate, Func<int, Task> progressReporter)
    {
        return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, minValidDate, messagePredicate, progressReporter);
    }

    public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, DateTime minTimestamp, Predicate<IMessage> messagePredicate)
    {
        return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, minTimestamp, messagePredicate, null);
    }
    
    public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, DateTime minTimestamp, Predicate<IMessage> messagePredicate, Func<int, Task> progressReporter)
    {
        return await GetMessageRangeAsync(channel, firstMessageID, lastMessageID, minTimestamp, messagePredicate, progressReporter?.WrapSync());
    }
    public static async Task<HashSet<IMessage>> GetMessageRangeAsync(this IMessageChannel channel, ulong firstMessageID, ulong lastMessageID, DateTime minTimestamp, Predicate<IMessage> messagePredicate, Action<int> progressReporter)
    {
        var result = new HashSet<IMessage>();

        // lastMessageID + 1 because the message retriever method retrieves messages before the given message's ID, excluding the original one
        // And even if not, there's close to 0 chance two messages with consecutive IDs are sent in the same channel in the real world
        for (ulong currentMaxID = lastMessageID + 1; currentMaxID > firstMessageID;)
        {
            var messages = await channel.GetMessagesAsync(currentMaxID, Direction.Before, DiscordConfig.MaxMessagesPerBatch).FirstAsync();

            bool containsEarlierMessages = false;
            bool foundNewMessages = false;
            foreach (var message in messages)
            {
                var id = message.Id;

                // Set the current ID to the minimum found ID
                if (id < currentMaxID)
                    currentMaxID = id;

                if (id < firstMessageID)
                    continue;

                if (message.Timestamp < minTimestamp)
                {
                    containsEarlierMessages = true;
                    continue;
                }    

                if (messagePredicate?.Invoke(message) == false)
                    continue;

                result.Add(message);
                foundNewMessages = true;
            }

            if (!foundNewMessages)
                break;

            progressReporter?.Invoke(result.Count);

            if (containsEarlierMessages)
                break;
        }

        return result;
    }

    public static async Task DeleteFoundMessages(this IMessageChannel messageChannel, IReadOnlyCollection<IMessage> foundMessages, DateTime currentUTCDiscordDateTime, Progress currentlyDeletedMessages)
    {
        IEnumerable<IMessage> seriallyDeletedMessages = foundMessages;

        if (messageChannel is ITextChannel textChannel)
        {
            var threshold = currentUTCDiscordDateTime - TimeSpan.FromDays(14);
            foundMessages.Dissect(m => m.Timestamp.UtcDateTime < threshold, out var olderMessages, out var newerMessages);

            var newerMessageIDs = newerMessages.Select(m => m.Id).ToArray();

            await textChannel.DeleteMessagesAsync(newerMessageIDs);

            currentlyDeletedMessages.Current += newerMessageIDs.Length;

            if (currentlyDeletedMessages.IsComplete)
                return;

            seriallyDeletedMessages = olderMessages;
        }

        foreach (var message in seriallyDeletedMessages)
        {
            await messageChannel.DeleteMessageAsync(message);
            currentlyDeletedMessages.Current++;
        }
    }
}
