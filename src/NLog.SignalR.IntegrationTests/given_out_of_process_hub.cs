using FluentAssertions;
using James.Testing;
using NLog.Config;
using NLog.SignalR.IntegrationTests.Hubs;
using NUnit.Framework;

namespace NLog.SignalR.IntegrationTests
{
    [TestFixture]
    public class given_out_of_process_hub : OutOfProcessHubFixture
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void SetUp()
        {
            Test.Current.SignalRLogEvents.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            Test.Current.SignalRLogEvents.Clear();
        }

        [Test]
        public void given_nlog_configured_to_use_signalr_target_for_hub_when_logging_events_should_log_to_signalr()
        {
            var target = new SignalRTarget
            {
                Name = "signalr",
                Uri = HubBaseUrl,
                Layout = "${message}"
            };
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            const string expectedMessage = "This is a sample trace message.";
            Logger.Trace(expectedMessage);
            Logger.Error(expectedMessage);

            Wait.Until(() => Test.Current.SignalRLogEvents.Count == 2);

            Test.Current.SignalRLogEvents.Should().Contain(x => x.Level == "Trace" && x.Message == expectedMessage);
            Test.Current.SignalRLogEvents.Should().Contain(x => x.Level == "Error" && x.Message == expectedMessage);
        }

        [Test]
        public void given_nlog_configured_to_use_signalr_target_for_hub_when_logging_events_after_server_shutdown_should_not_log_messages_after_shutdown()
        {
            var target = new SignalRTarget
            {
                Name = "signalr",
                Uri = HubBaseUrl,
                Layout = "${message}"
            };
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            const string expectedMessage = "This is a sample trace message.";
            Logger.Trace(expectedMessage);

            Wait.For(1).Seconds();

            StopHub();

            Logger.Error(expectedMessage);

            Wait.For(1).Seconds();

            Test.Current.SignalRLogEvents.Should().Contain(x => x.Level == "Trace" && x.Message == expectedMessage);
            Test.Current.SignalRLogEvents.Should().NotContain(x => x.Level == "Error" && x.Message == expectedMessage);

            StartHub();
        }

        [Test]
        public void given_nlog_configured_to_use_signalr_target_for_hub_when_logging_events_after_server_disconnects_and_reconnects_should_log_to_signalr()
        {
            var target = new SignalRTarget
            {
                Name = "signalr",
                Uri = HubBaseUrl,
                Layout = "${message}"
            };
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            const string expectedMessage = "This is a sample message.";
            Logger.Trace(expectedMessage);

            Wait.For(1).Seconds();

            StopHub();
            StartHub();

            Logger.Error(expectedMessage);

            Wait.Until(() => Test.Current.SignalRLogEvents.Count == 2);

            Test.Current.SignalRLogEvents.Should().Contain(x => x.Level == "Trace" && x.Message == expectedMessage);
            Test.Current.SignalRLogEvents.Should().Contain(x => x.Level == "Error" && x.Message == expectedMessage);
        }
    }
}