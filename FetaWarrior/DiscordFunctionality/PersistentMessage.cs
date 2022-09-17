using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Net;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

// Persistent message has potential; reduce the clutter it introduces when migrating over to interaction messages
public class PersistentMessage
{
    private readonly IDiscordInteraction interaction;
    public IUserMessage CurrentMessage { get; private set; }

    protected PersistentMessage() { }
    public PersistentMessage(IUserMessage currentMessage)
    {
        CurrentMessage = currentMessage;
    }
    public PersistentMessage(ISocketMessageChannel channel, string messageContent)
    {
        InitializeForChannel(channel, messageContent);
    }
    public PersistentMessage(IDiscordInteraction interaction)
    {
        CurrentMessage = interaction.GetOriginalResponseAsync().Result;
        this.interaction = interaction;
    }

    protected void InitializeForChannel(ISocketMessageChannel channel, string messageContent)
    {
        var message = channel.SendMessageAsync(messageContent).Result;
        CurrentMessage = message;
    }

    public async Task SetContentAsync(string content)
    {
        await ModifyAsync(m => m.Content = content);
    }

    public async Task ModifyAsync(Action<MessageProperties> modifier)
    {
        if (CurrentMessage is null && interaction is not null)
        {
            await interaction.RespondAsync("Beginning command execution...");
            CurrentMessage = await interaction.GetOriginalResponseAsync();
        }

        int failures = 0;
        while (true)
        {
            try
            {
                await CurrentMessage.ModifyAsync(modifier);
                return;
            }
            catch (HttpException e) when (e.HttpCode == HttpStatusCode.NotFound)
            {
                CurrentMessage = await CurrentMessage.Channel.SendMessageAsync(CurrentMessage.Content);
                await ModifyAsync(modifier);
            }
            catch
            {
                failures++;
            }

            if (failures > 6)
            {
                try
                {
                    await CurrentMessage.Channel.SendMessageAsync("Something went horribly wrong during the process of updating the message. The command will continue execution.");
                }
                catch
                {
                    // Okay what the fuck; we're trolling now
                }
            }

            await Task.Delay(200);
        }
    }

    public async Task DeleteAsync()
    {
        while (true)
        {
            try
            {
                await CurrentMessage.DeleteAsync();
                return;
            }
            catch (HttpException e) when (e.HttpCode == HttpStatusCode.NotFound)
            {
                return;
            }
            catch { }

            await Task.Delay(200);
        }
    }
}
