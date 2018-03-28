using System;
using System.IO;

namespace Spider
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Exception = 4
    }
    
    public class Logger : IDisposable
    {
        private readonly StreamWriter _writer;
        
        public LogLevel MinLogLevel { get; set; } = LogLevel.Info;

        public Logger(string filePath)
        {
            _writer = new StreamWriter(filePath, append:true);
        }
        
        public void Dispose()
        {
            _writer?.Dispose();
        }

        public void Log(string message, LogLevel logLevel)
        {
            if ((int) logLevel < (int) MinLogLevel)
            {
                return;
            }

            Console.WriteLine(message);
            _writer.WriteLine($"{DateTime.Now:G} [{logLevel.ToString().ToUpperInvariant()}] " + message);
        }

        public void LogDebug(string message)
        {
            Log(message, LogLevel.Debug);
        }

        public void LogInfo(string message)
        {
            Log(message, LogLevel.Info);
        }

        public void LogWarning(string message)
        {
            Log(message, LogLevel.Warning);
        }

        public void LogError(string error)
        {
            Log(error, LogLevel.Error);
        }

        public void LogException(Exception exception)
        {
            Log(exception.ToString(), LogLevel.Exception);
        }
    }
}