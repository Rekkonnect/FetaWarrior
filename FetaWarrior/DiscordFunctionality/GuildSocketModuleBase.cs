using Discord;
using Discord.Commands;

namespace FetaWarrior.DiscordFunctionality
{
    public class GuildSocketModuleBase : ModuleBase<SocketCommandContext>
    {
        public IGuildUser GuildUser => Context.Guild.GetUser(Context.User.Id);
        public string AuthorUsername => GuildUser.Username;
        public string AuthorNickname => GuildUser.Nickname;
        public string AuthorNicknameOrUsername => AuthorNickname ?? AuthorUsername;
    }
}
