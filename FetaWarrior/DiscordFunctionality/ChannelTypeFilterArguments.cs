using Discord;
using System.Collections.Generic;

namespace FetaWarrior.DiscordFunctionality;

public struct ChannelTypeFilterArguments
{
    public static ChannelTypeFilterArguments ForAll => new() { All = true };

    public bool All { get; set; }
    public bool Text { get; set; }
    public bool Voice { get; set; }
    public bool Announcement { get; set; }
    public bool Stage { get; set; }
    public bool Forum { get; set; }

    public bool PassesFilter(IGuildChannel channel)
    {
        if (All)
            return true;

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
