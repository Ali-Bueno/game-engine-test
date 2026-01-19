using System;
using System.IO;

namespace Game3
{
    public static class Program
    {
        private static string logFile = "game_log.txt";

        public static void Log(string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                File.AppendAllText(logFile, $"[{timestamp}] {message}\n");
            }
            catch { }
        }

        [STAThread]
        static void Main()
        {
            // Clear log file on start
            try { File.WriteAllText(logFile, $"=== Game started at {DateTime.Now} ===\n"); } catch { }

            try
            {
                Log("Creating Game1...");
                using var game = new Game1();
                Log("Game1 created, calling Run()...");
                game.Run();
                Log("Game ended normally.");
            }
            catch (Exception ex)
            {
                Log("=== CRASH ===");
                Log(ex.ToString());
                Log("=============");
            }
        }
    }
}
