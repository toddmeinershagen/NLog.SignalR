using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using James.Testing;
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
        public static readonly string HubBaseUrl = "http://localhost:80/Temporary_Listen_Addresses/" + Guid.NewGuid().ToString("D") + "/";
        public static readonly string RestBaseUrl = "http://localhost:80/Temporary_Listen_Addresses/" + Guid.NewGuid().ToString("D") + "/";

        [OneTimeSetUp]
        public void Init()
        {
            _host = new NancyHost(new Uri(RestBaseUrl));
            _host.Start();

            StartHub();
        }

        private static ManualResetEventSlim _mre;
        private static StringBuilder _output;

        protected void StartHub()
        {
            var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "NLog.SignalR.IntegrationTests.exe");
            _process = new Process
            {
                StartInfo =
                    new ProcessStartInfo(filePath, HubBaseUrl + " " + RestBaseUrl)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
            };

            _output = new StringBuilder();
            _process.OutputDataReceived += OutputHandler;
            _process.Disposed += DisposedHandler;

            _mre = new ManualResetEventSlim();
            _process.Start();
            _process.BeginOutputReadLine();
            _mre.Wait();

            Wait.For(3).Seconds();
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs args)
        {
            _output.Append(args.Data);
            if (_output.ToString() == "Service is listening...")
            {
                _mre.Set();
            }
        }

        private void DisposedHandler(object sender, EventArgs e)
        {
            _mre.Set();
        }

        protected void StopHub()
        {
            if (_process == null)
                return;

            _mre = new ManualResetEventSlim();
            _process.Kill();
            _process.WaitForExit();
            _process.Dispose();
            _mre.Wait();

            _process = null;
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            StopHub();

            _host?.Dispose();
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

                Test.Current.SignalRLogEvents.Push(logEvent);
                return Negotiate
                    .WithModel(logEvent)
                    .WithStatusCode(HttpStatusCode.Created);
            };
        }
    }
}