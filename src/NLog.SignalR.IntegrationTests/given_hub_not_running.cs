using System.Threading;
using FluentAssertions;
using NLog.Config;
using NLog.SignalR.IntegrationTests.Hubs;
using NUnit.Framework;

namespace NLog.SignalR.IntegrationTests
{
    [TestFixture]
    public class given_hub_not_running
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Test]
        public void given_nlog_configured_to_use_signalr_target_for_hub_when_logging_events_should_not_log_to_signalr()
        {
            var target = new SignalRTarget
            {
                Name = "signalr",
                Uri = InProcessHubFixture.HubBaseUrl,
                Layout = "${message}"
            };
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            const string expectedMessage = "This is a sample trace message.";
            Logger.Trace(expectedMessage);

            Thread.Sleep(1000);

            Test.Current.SignalRLogEvents.Should()
                .NotContain(x => x.Level == "Trace" && x.Message == expectedMessage);
        }
    }
}
