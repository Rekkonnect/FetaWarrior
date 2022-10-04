using Discord;
using System.Collections.Generic;

namespace FetaWarrior.DiscordFunctionality;

public record struct ChannelTypeFilterArguments(bool Text, bool Voice, bool Announcement, bool Stage, bool Forum)
{
    public bool PassesFilter(IGuildChannel channel)
    {
        return channel.GetChannelType() switch
        {
            ChannelType.Text => Text,
            ChannelType.Voice => Voice,
            ChannelType.News => Announcement,
            ChannelType.Stage => Stage,
            ChannelType.Forum => Forum,

            _ => false,
        };
    }

    public void SetForChannelType(NestedChannelType nestedChannelType, bool enabled)
    {
        // Ugly as fuck because no switch on refs + no property refs
        switch (nestedChannelType)
        {
            case NestedChannelType.Text: 
                Text = enabled;
                break;

            case NestedChannelType.Voice:
                Voice = enabled;
                break;

            case NestedChannelType.Announcement:
                Announcement = enabled;
                break;

            case NestedChannelType.Stage:
                Stage = enabled;
                break;

            case NestedChannelType.Forum:
                Forum = enabled;
                break;
        };
    }

    public static ChannelTypeFilterArguments FromTypes(IEnumerable<NestedChannelType> channelTypes)
    {
        var result = new ChannelTypeFilterArguments();

        foreach (var channelType in channelTypes)
        {
            result.SetForChannelType(channelType, true);
        }

        return result;
    }
}
