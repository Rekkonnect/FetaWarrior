using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public class FunModule : SocketModule
{
    #region Hugs
    [Command("hug")]
    [Summary("Hugs you. This command was added because the developer would greatly appreciate some hugs.")]
    public async Task HugAsync()
    {
        await Context.Channel.SendMessageAsync($"Huggie with **{AuthorNicknameOrUsername}** :heart:");
    }
    [Command("hug")]
    [Summary("Hugs a specified user. This command was added because the developer would greatly appreciate some hugs.")]
    public async Task HugAsync
    (
        [Summary("The user to hug.")]
        IUser user
    )
    {
        await Context.Channel.SendMessageAsync($"{user.Mention}, **{AuthorNicknameOrUsername}** hugs you :heart:");
    }
    #endregion
}
