using Discord;
using Discord.Commands;

namespace FetaWarrior.DiscordFunctionality
{
    public class SocketModule : ModuleBase<SocketCommandContext>
    {
        public IGuildUser GuildUser => Context.Guild?.GetUser(Context.User.Id);
        public string AuthorUsername => Context.User.Username;
        public string AuthorNickname => GuildUser?.Nickname;
        public string AuthorNicknameOrUsername => AuthorNickname ?? AuthorUsername;
    }
}
