using System;

namespace NLog.SignalR.Sample.Command
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static LogEventInfo[] Messages =
        {
            new LogEventInfo{Level = LogLevel.Warn, Message = "Warning - you might not want to do that..."},
            new LogEventInfo{Level = LogLevel.Fatal, Message = "The sky is falling."},
            new LogEventInfo{Level = LogLevel.Debug, Message = "You have entered the static void Main(string[] args)."}
        };

        static void Main(string[] args)
        {
            var generator = new Random();

            Console.WriteLine("Press the ESC key to quit.  Press any other key to log a random message type.\n");
            while (UserWantsToLog())
            {
                var index = generator.Next(0, Messages.Length);
                var message = Messages[index];
                Logger.Log(message.Level, message.Message);
            }
        }

        static bool UserWantsToLog()
        {
            var key = Console.ReadKey().Key;
            Console.WriteLine();
            return key != ConsoleKey.Escape;
        }
    }        
}
