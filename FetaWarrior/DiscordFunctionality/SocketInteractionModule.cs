using Discord;
using Discord.Interactions;
using FetaWarrior.Extensions;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public abstract class SocketInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    public IGuildUser GuildUser => Context.Guild?.GetUser(Context.User.Id);
    public string AuthorUsername => Context.User.Username;
    public string AuthorNickname => GuildUser?.Nickname;
    public string AuthorNicknameOrUsername => AuthorNickname ?? AuthorUsername;

    protected async Task<bool> ValidateChannel(IMessageChannel textChannel)
    {
        // Theoretically, this should never fail now, unless I miss some other preconditions
        if (textChannel is not IGuildChannel guildChannel || guildChannel.GuildId != Context.Guild.Id)
        {
            await RespondAsync("This server does not contain the provided channel.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that the provided IDs are valid and in the correct order.
    /// </summary>
    /// <param name="firstMessageID">The first message ID.</param>
    /// <param name="lastMessageID">The last message ID.</param>
    /// <returns><see langword="true"/> if the message IDs are valid, <see langword="false"/> otherwise.</returns>
    protected async Task<bool> ValidateMessageIDs(Snowflake firstMessageID, Snowflake lastMessageID)
    {
        if (lastMessageID == 0)
        {
            await RespondAsync("The last message ID cannot be 0.");
            return false;
        }

        if (firstMessageID > lastMessageID)
        {
            await RespondAsync("The first message ID cannot be greater than the last message ID.");
            return false;
        }

        return true;
    }

    protected async Task UpdateResponseTextAsync(string message)
    {
        await InteractionModuleBaseExtensions.UpdateResponseTextAsync(this, message);
    }
}
