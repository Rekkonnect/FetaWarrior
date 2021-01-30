using Discord;
using Discord.Commands;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    public class UtilitiesModule : SocketModule
    {
        #region Help
        [Command("help")]
        [Alias("h")]
        [Summary("Displays a help message containing a list of all the available commands.")]
        public async Task HelpAsync()
        {
            var embedBuilder = new EmbedBuilder
            {
                Title = "Available Commands",
                Description = "Use `help <command>` to get more help for individual commands.",
            };

            await WriteLongCommandList(CommandHandler.AllPubliclyAvailableCommands, embedBuilder);
        }

        [Command("help")]
        [Alias("h")]
        [Summary("Displays a message providing details about the matched command(s).")]
        public async Task HelpAsync
        (
            [Remainder]
            [Summary("The name of the command. Matches a command group name, a full subcommand name or a full command name.")]
            string commandName
        )
        {
            commandName = commandName.ToLower();
            var commands = CommandHandler.AllPubliclyAvailableCommands.Where(c => MatchCommand(c, commandName)).ToList();

            if (!commands.Any())
            {
                await ReplyAsync($"There is no command named {commandName}.");
                return;
            }

            if (commands.Count > 4)
            {
                var embedBuilder = new EmbedBuilder
                {
                    Title = "Matched Commands",
                    Description = "Use `help <command>` to get more help for individual commands.",
                };

                await WriteLongCommandList(commands, embedBuilder);
                return;
            }

            await ReplyAsync($"Commands that match {commandName}: ");

            foreach (var command in commands)
            {
                var currentEmbed = new EmbedBuilder();

                currentEmbed.Title = GetCommandSignature(command);
                currentEmbed.Description = GetCommandSummary(command);

                foreach (var parameter in command.Parameters)
                    currentEmbed.AddField(GetParameterSignature(parameter), GetParameterSummary(parameter));

                currentEmbed.AddField("Aliases", command.Aliases.Select(a => $"`{a}`").Combine("\n"));

                await ReplyAsync(embed: currentEmbed.Build());
            }
        }

        private async Task WriteLongCommandList(IEnumerable<CommandInfo> commands, EmbedBuilder embedBuilder)
        {
            var stringBuilder = new StringBuilder("```");
            foreach (var command in commands)
                stringBuilder.AppendLine($"{GetCommandSignature(command)}");

            stringBuilder.Append("```");
            embedBuilder.AddField("Command List", stringBuilder);

            await ReplyAsync(embed: embedBuilder.Build());
        }

        private static bool MatchCommand(CommandInfo command, string match)
        {
            if (command.Module.Group == match)
                return true;

            if (command.Module.Group?.StartsWith(match) == true)
                return true;

            foreach (var alias in command.Aliases)
            {
                if (alias.StartsWith(match))
                    return true;

                if (alias == match)
                    return true;

                if ($"{command.Module.Group} {alias}" == match)
                    return true;
            }

            return false;
        }

        private static string GetCommandSignature(CommandInfo command)
        {
            return $"{command.Module.Group ?? ""} {command.Name} {command.Parameters.Select(p => $"<{p.Name}>").CombineWords()}".TrimStart();
        }
        private static string GetParameterSignature(ParameterInfo parameter)
        {
            var signature = $"{parameter.Name} - {parameter.Type.Name}";
            if (parameter.IsOptional)
                signature += " (Optional)";
            return signature;
        }

        private static string GetCommandSummary(CommandInfo command)
        {
            return command.Summary ?? "No description available.";
        }
        private static string GetParameterSummary(ParameterInfo parameter)
        {
            return parameter.Summary ?? "No description available.";
        }
        #endregion

        #region Invite
        [Command("invite")]
        [Alias("inv")]
        [Summary("Gets the invite link for this bot.")]
        public async Task InviteAsync()
        {
            await ReplyAsync(InviteUtilities.GenerateBotInviteLink(BotCredentials.Instance.ClientID, (ulong)BotClientManager.MinimumBotPermissions));
        }
        [Command("invite")]
        [Alias("inv")]
        [Summary("Gets the invite link for a bot, requesting admin permissions.")]
        public async Task InviteAsync
        (
            [Summary("The ID of the bot whose invite to request.")]
            ulong botID
        )
        {
            await ReplyAsync(InviteUtilities.GenerateBotInviteLinkAdminPermissions(botID));
        }
        [Command("invite")]
        [Alias("inv")]
        [Summary("Gets the invite link for a bot, requesting the specified permissions.")]
        public async Task InviteAsync
        (
            [Summary("The ID of the bot whose invite to request.")]
            ulong botID,
            [Summary("The permissions integer for the bot. Refer to the Discord API for how to get this number.")]
            ulong permissions
        )
        {
            await ReplyAsync(InviteUtilities.GenerateBotInviteLink(botID, permissions));
        }
        #endregion

        #region Repository
        [Command("repository")]
        [Alias("repo")]
        [Summary("Gets the link to this bot's source code repository on GitHub.")]
        public async Task ShowRepositoryAsync()
        {
            await ReplyAsync("https://github.com/AlFasGD/FetaWarrior");
        }
        #endregion

        #region Ping
        [Command("ping")]
        [Summary("Gets the current ping.")]
        public async Task PingAsync()
        {
            await ReplyAsync($"Current Ping: `{Context.Client.Latency}ms`");
        }
        #endregion
    }
}
