// Uncomment the following line to enable test commands within this module file
#define TEST

using Discord;
using Discord.Interactions;
using FetaWarrior.DiscordFunctionality.Interactions.Attributes;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public class TestModule : SocketInteractionModule
{
#if TEST
    [SlashCommand("test-admin", "A very private test that you should not be able to see.")]
#endif
    [RequireGuildContext]
    [RequireUserAdminPermission]
    public async Task TestAdminAsync()
    {
        var permissions = Context.Guild.GetUser(Context.Interaction.User.Id).GuildPermissions;
        bool isAdmin = permissions.Has(GuildPermission.Administrator);
        await ReplyAsync(isAdmin switch
        {
            true => "The command was successfully called by an admin; nothing wrong here.",
            false => "The command was unfortunately called by a non-admin, despite the admin requirement in the command's definition.",
        });
    }
}
