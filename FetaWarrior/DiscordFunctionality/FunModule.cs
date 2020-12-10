using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        #region Hugs
        [Command("hug")]
        [Summary("Hugs you. This command was added because the developer would greatly appreciate some hugs.")]
        public async Task HugAsync()
        {
            await Context.Channel.SendMessageAsync($"Huggie with {Context.Message.Author.Username} :heart:");
        }
        [Command("hug")]
        [Summary("Hugs a specified user. This command was added because the developer would greatly appreciate some hugs.")]
        public async Task HugAsync
        (
            [Name("user")]
            [Summary("The user to hug.")]
            IUser user
        )
        {
            await Context.Channel.SendMessageAsync($"{user.Mention}, {Context.Message.Author.Username} hugs you :heart:");
        }
        #endregion
    }
}
