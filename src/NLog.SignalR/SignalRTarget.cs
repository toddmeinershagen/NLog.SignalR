using System;
using System.ComponentModel;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.SignalR
{
    [Target("SignalR")]
    public class SignalRTarget : TargetWithLayout
    {
        [RequiredParameter]
        public Layout Uri { get; set; }

        [RequiredParameter]
        [DefaultValue("LoggingHub")]
        public Layout HubName { get; set; }

        [RequiredParameter]
        [DefaultValue("Log")]
        public Layout MethodName { get; set; }

        public readonly HubProxy Proxy;

        public SignalRTarget()
        {
            HubName = "LoggingHub";
            MethodName = "Log";
            Proxy = new HubProxy();
#if NETSTANDARD
            OptimizeBufferReuse = true;
#endif
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var renderedMessage = this.Layout.Render(logEvent);
            var uri = this.Uri.Render(logEvent);
            var hubName = this.HubName.Render(logEvent);
            var methodName = this.MethodName.Render(logEvent);
            var item = new LogEvent(logEvent, renderedMessage);
            Proxy.Log(item, uri, hubName, methodName);
        }
    }
}
