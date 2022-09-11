using Discord;
using Discord.Commands;
using System;

namespace FetaWarrior.DiscordFunctionality;

[Obsolete("This is going to be removed sooner or later")]
public abstract class SocketModule : ModuleBase<SocketCommandContext>
{
    public IGuildUser GuildUser => Context.Guild?.GetUser(Context.User.Id);
    public string AuthorUsername => Context.User.Username;
    public string AuthorNickname => GuildUser?.Nickname;
    public string AuthorNicknameOrUsername => AuthorNickname ?? AuthorUsername;
}
