using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public class FunModule : SocketInteractionModule
{
    #region Hugs
    [SlashCommand("hug", "Give a hug!")]
    public async Task HugAsync
    (
        [Summary(description: "The user to hug.")]
        IUser user = null
    )
    {
        if (user is null)
        {
            await Context.Channel.SendMessageAsync($"Huggie with **{AuthorNicknameOrUsername}** :heart:");
        }
        else
        {
            await Context.Channel.SendMessageAsync($"{user.Mention}, **{AuthorNicknameOrUsername}** hugs you :heart:");
        }
    }
    #endregion
}
