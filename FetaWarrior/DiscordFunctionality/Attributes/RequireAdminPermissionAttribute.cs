using Discord.Commands;

namespace FetaWarrior.DiscordFunctionality.Attributes
{
    public class RequireAdminPermissionAttribute : RequireUserPermissionAttribute
    {
        public RequireAdminPermissionAttribute()
            : base(Discord.GuildPermission.Administrator)
        {
        }
    }
}
