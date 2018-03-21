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

    public class Logger
    {
        private static Logger _logger;
        private readonly string _logFile;

        public LogLevel MinLogLevel { get; set; } = LogLevel.Info;

        public Logger(string logFile)
        {
            this._logFile = logFile;
            File.AppendAllText(logFile, string.Empty);
        }

        public static Logger Instance
        {
            get
            {
                if (_logger == null)
                {
                    throw new Exception("Logger not initialized.");
                }

                return _logger;
            }
        }
        
        public static void Initialize(string logFile)
        {
            _logger = new Logger(logFile);
        }

        public void LogDebug(string message)
        {
            if ((int)LogLevel.Debug < (int)MinLogLevel)
            {
                return;
            }

            Console.WriteLine(message);
            File.AppendAllText(_logFile, $"\n{DateTime.Now:G} DEBUG: " + message);
        }

        public void LogInfo(string message)
        {
            if ((int)LogLevel.Info < (int)MinLogLevel)
            {
                return;
            }

            Console.WriteLine(message);
            File.AppendAllText(_logFile, $"\n{DateTime.Now:G} INFO: " + message);
        }

        public void LogWarning(string message)
        {
            if ((int)LogLevel.Warning < (int)MinLogLevel)
            {
                return;
            }

            Console.WriteLine(message);
            File.AppendAllText(_logFile, $"\n{DateTime.Now:G} WARNING: " + message);
        }

        public void LogError(string error)
        {
            if ((int)LogLevel.Error < (int)MinLogLevel)
            {
                return;
            }

            Console.WriteLine(error);
            File.AppendAllText(_logFile, $"\n{DateTime.Now:G} ERROR: " + error);
        }

        public void LogException(Exception exception)
        {
            if ((int)LogLevel.Exception < (int)MinLogLevel)
            {
                return;
            }

            Console.WriteLine(exception.Message);
            File.AppendAllText(_logFile, $"\n{DateTime.Now:G} EXCEPTION: " + exception.Message + "\n" + exception.StackTrace);
        }
    }
}