using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace FetaWarrior.Extensions;

public static class SocketGuildExtensions
{
    public static IEnumerable<SocketGuildChannel> UncategorizedChannels(this SocketGuild guild)
    {
        return guild.Channels.Where(IsUncategorized);

        static bool IsUncategorized(SocketGuildChannel channel)
        {
            return channel is INestedChannel nestedChannel
                && nestedChannel.CategoryId is null;
        }
    }

    public static IEnumerable<INestedChannel> NestedChannels(this SocketGuild guild)
    {
        return guild.Channels.OfType<INestedChannel>();
    }

    public static IEnumerable<IGrouping<ulong?, INestedChannel>> CategorizedNestedChannels(this SocketGuild guild)
    {
        return guild.NestedChannels().GroupBy(channel => channel.CategoryId);
    }

    public static IEnumerable<INestedChannel> ChannelsInCategory(this SocketGuild guild, ulong? categoryID)
    {
        return guild.NestedChannels().Where(channel => channel.CategoryId == categoryID);
    }
}
