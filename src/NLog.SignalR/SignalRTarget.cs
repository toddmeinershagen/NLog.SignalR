using System;
using System.ComponentModel;
using NLog.Common;
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

        public HubProxy Proxy { get; private set; }

        public SignalRTarget()
        {
            HubName = "LoggingHub";
            MethodName = "Log";
            Proxy = new HubProxy();
            OptimizeBufferReuse = true;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var renderedMessage = RenderLogEvent(this.Layout, logEvent);
            var uri = RenderLogEvent(this.Uri, logEvent);
            var hubName = RenderLogEvent(this.HubName, logEvent);
            var methodName = RenderLogEvent(this.MethodName, logEvent);
            var item = new LogEvent(logEvent, renderedMessage);
            Proxy.Log(item, uri, hubName, methodName);
        }

        protected override void CloseTarget()
        {
            var proxy = Proxy;
            Proxy = new HubProxy();
            proxy.Dispose();
        }
    }
}
