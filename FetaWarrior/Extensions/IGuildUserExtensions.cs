using Discord;
using FetaWarrior.DiscordFunctionality;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace FetaWarrior.Extensions;

public static class IGuildUserExtensions
{
    #region Voice-related
    public static async Task DisconnectAsync(this IGuildUser guildUser)
    {
        await guildUser.PerformVoiceActionAsync(VoiceChannelAction.Disconnect);
    }
    public static async Task MuteAsync(this IGuildUser guildUser)
    {
        await guildUser.PerformVoiceActionAsync(VoiceChannelAction.Mute);
    }
    public static async Task UnmuteAsync(this IGuildUser guildUser)
    {
        await guildUser.PerformVoiceActionAsync(VoiceChannelAction.Unmute);
    }
    public static async Task DeafenAsync(this IGuildUser guildUser)
    {
        await guildUser.PerformVoiceActionAsync(VoiceChannelAction.Deafen);
    }
    public static async Task UndeafenAsync(this IGuildUser guildUser)
    {
        await guildUser.PerformVoiceActionAsync(VoiceChannelAction.Undeafen);
    }
    public static async Task MuteDeafenAsync(this IGuildUser guildUser)
    {
        await guildUser.PerformVoiceActionAsync(VoiceChannelAction.MuteDeafen);
    }
    public static async Task UnmuteUndeafenAsync(this IGuildUser guildUser)
    {
        await guildUser.PerformVoiceActionAsync(VoiceChannelAction.UnmuteUndeafen);
    }

    public static async Task PerformVoiceActionAsync(this IGuildUser guildUser, VoiceChannelAction action)
    {
        await guildUser.ModifyAsync(GetModificationFunction(action));
    }

    private static Action<GuildUserProperties> GetModificationFunction(VoiceChannelAction action)
    {
        return action switch
        {
            VoiceChannelAction.Disconnect => Disconnect,
            VoiceChannelAction.Mute => Mute,
            VoiceChannelAction.Unmute => Unmute,
            VoiceChannelAction.Deafen => Deafen,
            VoiceChannelAction.Undeafen => Undeafen,
            VoiceChannelAction.MuteDeafen => MuteDeafen,
            VoiceChannelAction.UnmuteUndeafen => UnmuteUndeafen,

            _ => throw new InvalidEnumArgumentException("An invalid voice channel action was provided.")
        };
    }

    private static void Disconnect(GuildUserProperties properties)
    {
        properties.Channel = null;
    }
    private static void Mute(GuildUserProperties properties)
    {
        properties.Mute = true;
    }
    private static void Unmute(GuildUserProperties properties)
    {
        properties.Mute = false;
    }
    private static void Deafen(GuildUserProperties properties)
    {
        properties.Deaf = true;
    }
    private static void Undeafen(GuildUserProperties properties)
    {
        properties.Deaf = false;
    }
    private static void MuteDeafen(GuildUserProperties properties)
    {
        Mute(properties);
        Deafen(properties);
    }
    private static void UnmuteUndeafen(GuildUserProperties properties)
    {
        Unmute(properties);
        Undeafen(properties);
    }
    #endregion
}
