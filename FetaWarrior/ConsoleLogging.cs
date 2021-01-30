using Garyon.Functions;
using System;
using static System.Console;

namespace FetaWarrior
{
    public static class ConsoleLogging
    {
        private static object writingLock = new object();

        public static void WriteCurrentTime()
        {
            ConsoleUtilities.WriteWithColor($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss.ffff}] ", ConsoleColor.Yellow);
        }
        public static void WriteException(Exception e)
        {
            WriteLine($"Exception: {e.GetType()}\nMessage: {e.Message ?? "null"}\nStack trace:\n{e.StackTrace}");
            WriteLine();
            if (e.InnerException != null)
                WriteException(e.InnerException);
        }

        public static void WriteEventWithCurrentTime(string message)
        {
            lock (writingLock)
            {
                WriteCurrentTime();
                WriteLine($"{message}\n");
            }
        }
    }
}
