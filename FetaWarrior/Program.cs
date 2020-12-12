﻿using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FetaWarrior.Configuration;
using FetaWarrior.DiscordFunctionality;
using System;
using System.Threading.Tasks;
using static System.Console;
using static System.IO.File;
using static System.Threading.Thread;

namespace FetaWarrior
{
    public static class Program
    {
        public static DiscordSocketClient Client => CommandHandler.GlobalCommandHandler.Client;
        public static DiscordRestClient RestClient => CommandHandler.GlobalCommandHandler.RestClient;

        private static string clientID, clientSecret, botToken;

        public static void Main(string[] args)
        {
            GetSecretLoginStuff(); // NEVER publish secret things like these (keep them at a secret file no one reads shhh)
            Login();
            SetStatus();

            WriteLine("Press the ESC key to log out at any time.");
            while (ReadKey(true).Key != ConsoleKey.Escape);

            Logout();
        }

        private static void GetSecretLoginStuff()
        {
            string[] lines = ReadAllLines("secrets.txt");
            clientID = lines[0];
            clientSecret = lines[1];
            botToken = lines[2];
        }

        private static void SetStatus()
        {
            var lines = new string[]
            {
                $"Default Prefix: \"{BotConfig.DefaultPrefix}\"",
                $"Help Command: \"{BotConfig.DefaultPrefix}help\"",
            };

            SetActivityLoop();

            async Task SetActivityLoop()
            {
                int index = 0;

                while (true)
                {
                    await Client.SetActivityAsync(new Game(lines[index], ActivityType.Playing, details: lines[index]));
                    await Task.Delay(5000);
                    index = (index + 1) % lines.Length;
                }
            }
        }

        private static void Login()
        {
            var client = new DiscordSocketClient();
            var restClient = new DiscordRestClient();

            CommandHandler.InitializeSingletonFromClient(client, restClient);

            PerformAsyncTask(Client.LoginAsync(TokenType.Bot, botToken));
            PerformAsyncTask(Client.StartAsync());
            Client.SetStatusAsync(UserStatus.Online);

            PerformAsyncTask(RestClient.LoginAsync(TokenType.Bot, botToken));

            WriteLine("Logged in.");
        }
        private static void Logout()
        {
            PerformAsyncTask(Client.SetStatusAsync(UserStatus.Offline));
            PerformAsyncTask(Client.StopAsync());
            PerformAsyncTask(Client.LogoutAsync());

            PerformAsyncTask(RestClient.LogoutAsync());
        }

        private static void PerformAsyncTask(Task task)
        {
            while (task.Status < TaskStatus.RanToCompletion)
                Sleep(250);
        }
    }
}
