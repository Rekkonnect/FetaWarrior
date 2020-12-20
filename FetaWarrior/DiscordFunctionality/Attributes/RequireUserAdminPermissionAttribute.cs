using Discord.Commands;
using System;
using static Discord.GuildPermission;

namespace FetaWarrior.DiscordFunctionality.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireUserAdminPermissionAttribute : RequireUserPermissionAttribute
    {
        public RequireUserAdminPermissionAttribute()
            : base(Administrator) { }
    }
}
