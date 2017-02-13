using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NLog.SignalR.IntegrationTests.Hubs
{
    public class Test
    {
        private static readonly Lazy<Test> Instance = new Lazy<Test>(() => new Test());

        private Test()
        {
            SignalRLogEvents = new ConcurrentStack<LogEvent>();
        }

        public static Test Current => Instance.Value;

        public ConcurrentStack<LogEvent> SignalRLogEvents { get; private set; }
    }
}
