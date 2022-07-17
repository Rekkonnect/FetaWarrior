using FetaWarrior.DiscordFunctionality;
using System;
using static System.Console;

namespace FetaWarrior;

public static class Program
{
    public static void Main(string[] args)
    {
        BotClientManager.Instance.InitializeLogin().Wait();

        WriteLine("Press the ESC key to log out at any time.\n");
        AwaitPressedKey(ConsoleKey.Escape);

        BotClientManager.Instance.Logout().Wait();
    }

    private static void AwaitPressedKey(ConsoleKey key)
    {
        while (ReadKey(true).Key != key);
    }
}
