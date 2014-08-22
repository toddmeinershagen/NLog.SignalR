using Microsoft.AspNet.SignalR;

namespace NLog.SignalR.Sample.Web.Hubs
{
    public class LoggingHub : Hub<ILoggingHub>
    {
        public void Log(LogEvent logEvent)
        {
            Clients.Others.Log(logEvent);
        }
    }
}