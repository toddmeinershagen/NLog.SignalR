using System;
using System.Diagnostics;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.ModelBinding;
using NUnit.Framework;

namespace NLog.SignalR.IntegrationTests.Hubs
{
    public class OutOfProcessHubFixture
    {
        private NancyHost _host;
        private Process _process;
        public const string HubBaseUrl = "http://localhost:1235";
        public const string RestBaseUrl = "http://localhost:1236";

        [TestFixtureSetUp]
        public void Init()
        {
            _host = new NancyHost(new Uri(RestBaseUrl));
            _host.Start();

            StartHub();
        }

        protected void StartHub()
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo("NLog.SignalR.IntegrationTests.exe", HubBaseUrl) {UseShellExecute = false}
            };
            _process.Start();
        }

        protected void StopHub()
        {
            if (_process == null)
                return;

            _process.Kill();
            _process.Dispose();
            _process = null;
        }

        [TestFixtureTearDown]
        public void Dispose()
        {
            StopHub();

            if (_host == null)
                return;
            _host.Dispose();
        }
    }

    public class SignalRLogEventsModule : NancyModule
    {
        public SignalRLogEventsModule()
            : base("SignalRLogEvents")
        {
            Post["/"] = _ =>
            {
                var logEvent = this.Bind<LogEvent>();
                Test.Current.SignalRLogEvents.Add(logEvent);
                return Negotiate
                    .WithModel(logEvent)
                    .WithStatusCode(HttpStatusCode.Created);
            };
        }
    }
}