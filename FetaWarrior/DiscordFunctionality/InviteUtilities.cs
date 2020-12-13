using Discord;

namespace FetaWarrior.DiscordFunctionality
{
    public abstract class InviteUtilities
    {
        public static string GenerateBotInviteLink(ulong clientID, GuildPermission permissions)
        {
            return $"https://discord.com/api/oauth2/authorize?client_id={clientID}&permissions={(ulong)permissions}&scope=bot";
        }
        public static string GenerateBotInviteLinkAdminPermissions(ulong clientID)
        {
            return GenerateBotInviteLink(clientID, GuildPermission.Administrator);
        }
    }
}
