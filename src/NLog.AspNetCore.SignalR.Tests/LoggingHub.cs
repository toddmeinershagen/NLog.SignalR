using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NLog.SignalR;

namespace NLog.AspNetCore.SignalR.Tests
{
    public class LoggingHub : Hub<ILoggingHub>
    {
        public static readonly ConcurrentQueue<LogEvent> LogEvents = new ConcurrentQueue<LogEvent>();

        public async Task Log(LogEvent logEvent)
        {
            LogEvents.Enqueue(logEvent);
            await Task.Delay(10);
        }
    }
}
