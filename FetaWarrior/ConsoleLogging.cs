using System;
using static System.Console;

namespace FetaWarrior
{
    public static class ConsoleLogging
    {
        private static object writingLock = new object();

        public static void WriteCurrentTime()
        {
            Write($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss.ffff}] ");
        }
        public static void WriteException(Exception e)
        {
            WriteLine($"Exception: {e.GetType()}\nMessage: {e.Message ?? "null"}\nStack trace:\n{e.StackTrace}");
            if (e.InnerException != null)
                WriteException(e.InnerException);
            WriteLine();
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
