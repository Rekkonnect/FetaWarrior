using Discord;
using Discord.Interactions;
using FetaWarrior.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

[EnabledInDm(false)]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(VoicePermissions)]
[RequireBotPermission(VoicePermissions)]
public class VoiceChannelModule : MassYeetUsersModuleBase
{
    public const GuildPermission VoicePermissions = GuildPermission.MoveMembers
                                                  | GuildPermission.MuteMembers
                                                  | GuildPermission.DeafenMembers;

    private IVoiceChannel targetVoice;
    public VoiceChannelAction Action { get; private set; }

    public override UserYeetingLexemes Lexemes => Action switch
    {
        VoiceChannelAction.Move => MovingLexemes.Instance,
        VoiceChannelAction.Disconnect => DisconnectingLexemes.Instance,
        VoiceChannelAction.Mute => MutingLexemes.Instance,
        VoiceChannelAction.Unmute => UnmutingLexemes.Instance,
        VoiceChannelAction.Deafen => DeafeningLexemes.Instance,
        VoiceChannelAction.Undeafen => UndeafeningLexemes.Instance,
        VoiceChannelAction.MuteDeafen => MutingDeafeningLexemes.Instance,
        VoiceChannelAction.UnmuteUndeafen => UnmutingUndeafeningLexemes.Instance,
    };

    [EnabledInDm(false)]
    [SlashCommand("move", "Move all members in a voice channel into another")]
    public async Task MoveAllUsers
    (
        [Summary(description: "The voice channel whose users are being moved")]
        IVoiceChannel originalVoiceChannel,
        [Summary(description: "The new voice channel to move all the users to")]
        IVoiceChannel targetVoiceChannel
    )
    {
        if (originalVoiceChannel.GuildId != Context.Guild.Id ||
            targetVoiceChannel.GuildId != Context.Guild.Id)
        {
            await RespondAsync("Both voice channels must be in this server that the interaction was created.");
            return;
        }

        if (originalVoiceChannel.Id == targetVoiceChannel.Id)
        {
            await RespondAsync("The voice chnanels must differ.");
            return;
        }

        await GenericAction(originalVoiceChannel, targetVoiceChannel, VoiceChannelAction.Move);
    }
    [EnabledInDm(false)]
    [SlashCommand("vc", "Perform an action to all users in a voice channel")]
    public async Task GenericAction
    (
        [Summary(description: "The voice channel whose users are being handled")]
        IVoiceChannel voiceChannel,
        [Summary(description: "The action to perform to all users")]
        VoiceChannelAction action
    )
    {
        if (voiceChannel.GuildId != Context.Guild.Id)
        {
            await RespondAsync("The voice chnanel must be in this server that the interaction was created.");
            return;
        }

        await GenericAction(voiceChannel, null, action);
    }

    private async Task GenericAction(IVoiceChannel originalVoiceChannel, IVoiceChannel targetVoiceChannel, VoiceChannelAction action)
    {
        await RespondAsync($"Discovering the users to {Lexemes.ActionName}...");

        Action = action;
        targetVoice = targetVoiceChannel;
        var users = (await originalVoiceChannel.GetUsersAsync(CacheMode.AllowDownload).FlattenAsync())
            .Where(user => user.VoiceChannel == originalVoiceChannel)
            .ToArray();

        await MassYeetWithProgress(users);
    }

    protected override async Task YeetUser(IUser user, string reason)
    {
        var guildUser = user as IGuildUser;

        switch (Action)
        {
            case VoiceChannelAction.Move:
                await Context.Guild.MoveAsync(guildUser, targetVoice);
                return;

            default:
                await guildUser.PerformVoiceActionAsync(Action);
                return;
        }
    }

    private sealed class MovingLexemes : UserYeetingLexemes
    {
        public static MovingLexemes Instance { get; } = new();
        private MovingLexemes() { }

        public override string ActionName => "move";
        public override string ActionPastParticiple => "moved";
    }
    private sealed class DisconnectingLexemes : UserYeetingLexemes
    {
        public static DisconnectingLexemes Instance { get; } = new();
        private DisconnectingLexemes() { }

        public override string ActionName => "disconnect";
        public override string ActionPastParticiple => "disconnected";
    }
    private sealed class MutingLexemes : UserYeetingLexemes
    {
        public static MutingLexemes Instance { get; } = new();
        private MutingLexemes() { }

        public override string ActionName => "mute";
        public override string ActionPastParticiple => "muted";
    }
    private sealed class UnmutingLexemes : UserYeetingLexemes
    {
        public static UnmutingLexemes Instance { get; } = new();
        private UnmutingLexemes() { }

        public override string ActionName => "unmute";
        public override string ActionPastParticiple => "unmuted";
    }
    private sealed class DeafeningLexemes : UserYeetingLexemes
    {
        public static DeafeningLexemes Instance { get; } = new();
        private DeafeningLexemes() { }

        public override string ActionName => "deafen";
        public override string ActionPastParticiple => "deafened";
    }
    private sealed class UndeafeningLexemes : UserYeetingLexemes
    {
        public static UndeafeningLexemes Instance { get; } = new();
        private UndeafeningLexemes() { }

        public override string ActionName => "undeafen";
        public override string ActionPastParticiple => "undeafened";
    }
    private sealed class MutingDeafeningLexemes : UserYeetingLexemes
    {
        public static MutingDeafeningLexemes Instance { get; } = new();
        private MutingDeafeningLexemes() { }

        public override string ActionName => "mute and deafen";
        public override string ActionPastParticiple => "muted and deafened";
    }
    private sealed class UnmutingUndeafeningLexemes : UserYeetingLexemes
    {
        public static UnmutingUndeafeningLexemes Instance { get; } = new();
        private UnmutingUndeafeningLexemes() { }

        public override string ActionName => "unmute and undeafen";
        public override string ActionPastParticiple => "unmuted and undeafened";
    }
}
