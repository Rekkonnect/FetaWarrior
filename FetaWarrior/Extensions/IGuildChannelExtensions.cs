using Discord;

namespace FetaWarrior.Extensions;
#nullable enable

public static class IGuildChannelExtensions
{
    public static ReorderChannelProperties ReorderInto(this IGuildChannel channel, int position)
    {
        return new(channel.Id, position);
    }
}
