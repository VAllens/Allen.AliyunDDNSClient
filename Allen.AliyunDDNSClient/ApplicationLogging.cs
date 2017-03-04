using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class ApplicationLogging
    {
        static ApplicationLogging()
        {
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddDebug().AddConsole().AddLog4Net();
        }
        
        public static ILoggerFactory LoggerFactory { get; }

        public static ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}
