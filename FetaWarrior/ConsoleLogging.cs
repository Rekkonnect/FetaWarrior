using System;
using static System.Console;

namespace FetaWarrior
{
    public static class ConsoleLogging
    {
        public static void WriteCurrentTime()
        {
            Write($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss.ffff}] ");
        }
        public static void WriteException(Exception e)
        {
            WriteLine($"Exception: {e.GetType()}\nMessage: {e.Message ?? "null"}\n\nStack trace:\n{e.StackTrace}");
            if (e.InnerException != null)
                WriteException(e.InnerException);
        }
    }
}
