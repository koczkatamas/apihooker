using System;

namespace LiveObjects.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void LogException(Exception e)
        {
            Console.WriteLine($"Exception: {e}");
        }

        public void Log(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}