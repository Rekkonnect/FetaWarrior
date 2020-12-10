using Discord;
using Discord.Commands;
using Garyon.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality
{
    /// <summary>Represents the help command's module.</summary>
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Summary("Displays a help message containing a list of all the available commands.")]
        public async Task HelpAsync()
        {
            EmbedBuilder embedBuilder = new EmbedBuilder
            {
                Title = "Available Commands",
                Description = "Use `help <command>` to get more help for individual commands.",
            };

            foreach (var command in CommandHandler.AllAvailableCommands)
            {
                string embedFieldText = command.Summary ?? "No description available.";
                embedBuilder.AddField(GetCommandSignature(command), embedFieldText);
            }

            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("help")]
        [Summary("Displays a message providing details about the requested command.")]
        public async Task HelpAsync
        (
            // I'm really not too sure about that kinda syntax
            [Remainder]
            [Name("commandName")]
            [Summary("The name of the command. Matches a command group name, a full subcommand name or a full command name.")]
            string commandName
        )
        {
            commandName = commandName.ToLower();
            var commands = CommandHandler.AllAvailableCommands.Where(c => MatchCommand(c, commandName)).ToList();

            if (!commands.Any())
            {
                await ReplyAsync($"There is no command named {commandName}.");
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

        private static bool MatchCommand(CommandInfo command, string match)
        {
            if (command.Module.Group == match)
                return true;

            foreach (var alias in command.Aliases)
            {
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
    }
}
