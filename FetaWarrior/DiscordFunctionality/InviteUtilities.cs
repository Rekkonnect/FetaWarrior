using Discord;

namespace FetaWarrior.DiscordFunctionality
{
    public abstract class InviteUtilities
    {
        public static string GenerateBotInviteLink(ulong clientID, ulong permissions)
        {
            return $"https://discord.com/api/oauth2/authorize?client_id={clientID}&permissions={permissions}&scope=bot";
        }
        public static string GenerateBotInviteLink(ulong clientID, GuildPermission permissions)
        {
            return GenerateBotInviteLink(clientID, (ulong)permissions);
        }
        public static string GenerateBotInviteLinkAdminPermissions(ulong clientID)
        {
            return GenerateBotInviteLink(clientID, GuildPermission.Administrator);
        }
    }
}
