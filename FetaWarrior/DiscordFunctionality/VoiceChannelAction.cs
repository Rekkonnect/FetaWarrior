using Discord.Interactions;

namespace FetaWarrior.DiscordFunctionality;

public enum VoiceChannelAction
{
    Disconnect,
    Mute,
    Unmute,
    Deafen,
    Undeafen,
    [ChoiceDisplay("Mute and Deafen")]
    MuteDeafen,
    [ChoiceDisplay("Unmute and Undeafen")]
    UnmuteUndeafen,

    // This is a separate command
    [Hide]
    Move,
}
