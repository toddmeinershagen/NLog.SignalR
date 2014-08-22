using System;
using System.ComponentModel;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace NLog.SignalR
{
    [Target("SignalR")]
    public class SignalRTarget : TargetWithLayout
    {
        public static SignalRTarget Instance = new SignalRTarget();
        public Action<String, LogEventInfo> LogEventHandler;

        [RequiredParameter]
        public string Uri { get; set; }

        [DefaultValue("LoggingHub")]
        public string HubName { get; set; }

        [DefaultValue("Log")]
        public string MethodName { get; set; }

        public readonly HubProxy Proxy;

        public SignalRTarget()
        {
            HubName = "LoggingHub";
            MethodName = "Log";
            Proxy = new HubProxy(this);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            Log(logEvent);
        }

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            Log(logEvent.LogEvent);
        }

        private void Log(LogEventInfo logEvent)
        {
            var renderedMessage = Layout.Render(logEvent);
            var item = new LogEvent(logEvent, renderedMessage);

            Proxy.Log(item);
        }
    }
}
