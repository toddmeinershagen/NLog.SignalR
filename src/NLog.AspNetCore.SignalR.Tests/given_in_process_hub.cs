using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NLog.Config;
using NLog.SignalR;
using NUnit.Framework;

namespace NLog.AspNetCore.SignalR.Tests
{
    [TestFixture]
    public class given_in_process_hub
    {
        public static readonly string HubBaseUrl = "http://localhost:80/Temporary_Listen_Addresses/" + Guid.NewGuid().ToString("D") + "/";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Task _host;
        private CancellationTokenSource _cancellationToken;

        [SetUp]
        public void SetUp()
        {
            var hubBaseUri = new Uri(HubBaseUrl);
            _cancellationToken = new CancellationTokenSource();
            _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(string.Format("http://*:{0}", hubBaseUri.Port))
                .ConfigureServices(services =>
                {
                    services.AddSignalR();
                })
                .Configure(app =>
                {
                    app.UsePathBase(hubBaseUri.LocalPath);
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapHub<LoggingHub>("/Logging");
                    });
                })
                .Build().RunAsync(_cancellationToken.Token);

            LoggingHub.LogEvents.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _cancellationToken?.Cancel();
            _host?.Wait(30000);
            LoggingHub.LogEvents.Clear();
        }

        [Test]
        public async Task given_nlog_configured_to_use_signalr_target_for_hub()
        {
            var target = new SignalRTarget
            {
                Name = "signalr",
                Uri = HubBaseUrl + "Logging",
                Layout = "${message}"
            };
            SimpleConfigurator.ConfigureForTargetLogging(target);

            const string expectedMessage = "This is a sample trace message.";
            Logger.Info(expectedMessage);

            LogManager.Shutdown();

            Assert.True(LoggingHub.LogEvents.TryDequeue(out var logEvent));
            Assert.AreEqual("Info", logEvent.Level);
            Assert.AreEqual(expectedMessage, logEvent.Message);
        }

        [Test]
        public void given_nlog_configured_to_use_signalr_target_for_hub_when_logging_events_after_client_disconnection_should_log_to_signalr()
        {
            var target = new SignalRTarget
            {
                Name = "signalr",
                Uri = HubBaseUrl + "Logging",
                Layout = "${message}"
            };
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            const string expectedMessage = "This is a sample trace message.";
            Logger.Info(expectedMessage);

            LogManager.Flush();

            target.Proxy.Stop();

            Logger.Error(expectedMessage);

            LogManager.Shutdown();

            Assert.True(LoggingHub.LogEvents.TryDequeue(out var logEvent));
            Assert.AreEqual("Info", logEvent.Level);
            Assert.AreEqual(expectedMessage, logEvent.Message);

            Assert.True(LoggingHub.LogEvents.TryDequeue(out logEvent));
            Assert.AreEqual("Error", logEvent.Level);
            Assert.AreEqual(expectedMessage, logEvent.Message);
        }
    }
}
