using System;
using System.Threading.Tasks;
using FluentAssertions;
using James.Testing;
using NLog.Config;
using NLog.SignalR.IntegrationTests.Hubs;
using NUnit.Framework;

namespace NLog.SignalR.IntegrationTests
{
    [TestFixture]
    public class given_in_process_hub : InProcessHubFixture
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
        public void given_nlog_configured_to_use_signalr_target_for_hub_when_logging_events_from_multiple_threads_should_log_to_signalr()
        {
            var target = new SignalRTarget
            {
                Name = "signalr",
                Uri = HubBaseUrl,
                Layout = "${message}"
            };
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            const string expectedMessage = "This is a sample trace message.";

            Action action1 = () =>
            {
                Logger.Trace(expectedMessage);
                Wait.For(1).Seconds();
            };

            Action action2 = () =>
            {
                Logger.Error(expectedMessage);
                Wait.For(1).Seconds();
            };

            Parallel.Invoke(action1, action2);

            Test.Current.SignalRLogEvents.Should().Contain(x => x.Level == "Trace" && x.Message == expectedMessage);
            Test.Current.SignalRLogEvents.Should().Contain(x => x.Level == "Error" && x.Message == expectedMessage);
        }

        [Test]
        public void given_nlog_configured_to_use_signalr_target_for_wrong_hub_uri_when_logging_events_should_not_log_to_signalr()
        {
            var target = new SignalRTarget
            {
                Name = "signalr",
                Uri = "http://localhost:1450",
                Layout = "${message}"
            };
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            const string expectedMessage = "This is a sample trace message.";
            Logger.Trace(expectedMessage);

            Wait.For(1).Seconds();

            Test.Current.SignalRLogEvents.Should().NotContain(x => x.Level == "Trace" && x.Message == expectedMessage);
        }

        [Test]
        public void given_nlog_configured_to_use_signalr_target_for_hub_with_wrong_hub_name_when_logging_events_should_not_log_to_signalr()
        {
            var target = new SignalRTarget
            {
                Name = "signalr",
                Uri = HubBaseUrl,
                HubName = "TestHub",
                Layout = "${message}"
            };
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            const string expectedMessage = "This is a sample trace message.";
            Logger.Trace(expectedMessage);

            Wait.For(1).Seconds();

            Test.Current.SignalRLogEvents.Should().NotContain(x => x.Level == "Trace" && x.Message == expectedMessage);
        }

        [Test]
        public void given_nlog_configured_to_use_signalr_target_for_hub_with_wrong_method_name_when_logging_events_should_not_log_to_signalr()
        {
            var target = new SignalRTarget
            {
                Name = "signalr",
                Uri = HubBaseUrl,
                MethodName = "Test",
                Layout = "${message}"
            };
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            const string expectedMessage = "This is a sample trace message.";
            Logger.Trace(expectedMessage);

            Wait.For(1).Seconds();

            Test.Current.SignalRLogEvents.Should().NotContain(x => x.Level == "Trace" && x.Message == expectedMessage);
        }

        [Test]
        public void given_nlog_configured_to_use_signalr_target_for_hub_when_logging_events_after_client_disconnection_should_log_to_signalr()
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

            target.Proxy.Connection.Stop();

            Logger.Error(expectedMessage);

            Wait.For(1).Seconds();

            Test.Current.SignalRLogEvents.Should().Contain(x => x.Level == "Trace" && x.Message == expectedMessage);
            Test.Current.SignalRLogEvents.Should().Contain(x => x.Level == "Error" && x.Message == expectedMessage);
        }
    }
}