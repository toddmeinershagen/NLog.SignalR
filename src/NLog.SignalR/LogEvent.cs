using System;

namespace NLog.SignalR
{
    public class LogEvent
    {
        public string Level { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }

        public LogEvent()
        {}

        internal LogEvent(LogEventInfo eventInfo, string renderedMessage)
        {
            Level = eventInfo.Level.Name;
            TimeStamp = eventInfo.TimeStamp.ToUniversalTime();
            Message = renderedMessage;
        }
    }
}