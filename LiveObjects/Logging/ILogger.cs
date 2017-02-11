using System;

namespace LiveObjects.Logging
{
    public interface ILogger
    {
        void LogException(Exception e);
        void Log(string msg);
    }
}