using System;

namespace LiveObjects.Logging
{
    public class ConsoleLogger : ILogger
    {
        public string Prefix { get; set; }

        public void LogException(Exception e)
        {
            Log($"Exception: {e}");
        }

        public void Log(string msg)
        {
            Console.WriteLine((String.IsNullOrEmpty(Prefix) ? "" : $"[{Prefix}] ") + msg);
        }
    }
}