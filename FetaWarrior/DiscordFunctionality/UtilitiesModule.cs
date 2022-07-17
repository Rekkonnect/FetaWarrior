﻿using Discord;
using Discord.Commands;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public class UtilitiesModule : SocketModule
{
    #region Help
    [Command("help")]
    [Alias("h")]
    [Summary("Displays a help message containing a list of all the available commands.")]
    public async Task HelpAsync()
    {
        var commands = CommandHandler.AllPubliclyAvailableCommands.ToList();
        var embedBuilder = new HelpEmbedBuilder(DisplayedCommandFilter.Available, commands);

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
            var embedBuilder = new HelpEmbedBuilder(DisplayedCommandFilter.Matched, commands);
            await WriteLongCommandList(commands, embedBuilder);
            return;
        }

        await SendAvailableCommands(commandName, commands);
    }

    private async Task SendAvailableCommands(string commandName, IReadOnlyCollection<CommandInfo> commands)
    {
        await ReplyAsync($"Found {commands.Count} commands that match {commandName}: ");

        var replyTasks = new List<Task>();
        foreach (var command in commands)
        {
            var currentEmbed = new EmbedBuilder();

            currentEmbed.Title = GetCommandSignature(command);
            currentEmbed.Description = GetCommandSummary(command);

            if (command.Remarks?.Any() == true)
                currentEmbed.AddField("Remarks", command.Remarks, true);

            foreach (var parameter in command.Parameters)
                currentEmbed.AddField(GetParameterSignature(parameter), GetParameterSummary(parameter));

            currentEmbed.AddField("Aliases", command.Aliases.Select(a => $"`{a}`").Combine("\n"));

            var replyTask = ReplyAsync(embed: currentEmbed.Build());
            replyTasks.Add(replyTask);
        }

        await replyTasks.WaitAll();
    }

    private async Task WriteLongCommandList(IEnumerable<CommandInfo> commands, HelpEmbedBuilder embedBuilder)
    {
        var stringBuilder = new StringBuilder("```");
        var distinctCommandNames = commands.Select(GetFullCommandName).Distinct();
        foreach (var commandName in distinctCommandNames)
            stringBuilder.AppendLine(commandName);

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

    private static string GetFullCommandName(CommandInfo command)
    {
        return $"{command.Module.Group} {command.Name}".TrimStart();
    }
    private static string GetCommandSignature(CommandInfo command)
    {
        return $"{GetFullCommandName(command)} {command.Parameters.Select(p => $"<{p.Name}>").CombineWords()}";
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

    private sealed class HelpEmbedBuilder : EmbedBuilder
    {
        private readonly DisplayedCommandFilter filter;
        private readonly IReadOnlyCollection<CommandInfo> commands;
        
        public HelpEmbedBuilder(DisplayedCommandFilter displayedCommandFilter, IReadOnlyCollection<CommandInfo> commands)
        {
            filter = displayedCommandFilter;
            this.commands = commands;

            ApplyHeaderInformation();
        }

        private void ApplyHeaderInformation()
        {
            Title = $"{filter} Commands: {commands.Count}";
            Description = $"Use `help <command>` to get more help for individual commands.";
        }
    }
    private enum DisplayedCommandFilter
    {
        Available,
        Matched,
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
