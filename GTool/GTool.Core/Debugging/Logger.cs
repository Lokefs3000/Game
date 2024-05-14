using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Debugging
{
    public class Logger
    {
        private static readonly ILogger _logger = new LoggerConfiguration()
            .WriteTo.Console()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .CreateLogger();

        private static readonly Logger _containedLogger = new Logger();

        internal Logger()
        {
            Log.Logger = _logger;
        }

        internal static void Dispose()
        {
            Log.CloseAndFlush();
        }

        public static void Debug(string message) => Log.Debug(message);
        public static void Debug(string message, params object?[]? objects) => Log.Debug(message, objects);

        public static void Information(string message) => Log.Information(message);
        public static void Information(string message, params object?[]? objects) => Log.Information(message, objects);

        public static void Warning(string message) => Log.Warning(message);
        public static void Warning(string message, params object?[]? objects) => Log.Warning(message, objects);

        public static void Error(string message) => Log.Error(message);
        public static void Error(string message, params object?[]? objects) => Log.Error(message, objects);

        public static void Fatal(string message) => Log.Fatal(message);
        public static void Fatal(string message, params object?[]? objects) => Log.Fatal(message, objects);

        public static void Assert(bool exp) => Assert(exp, "Assert failed!");
        public static void Assert(bool exp, string message)
        {
            if (!exp)
            {
                Logger.Fatal(message);
                throw new Exception(message);
            }
        }
    }
}
