using NUnit.Framework;

namespace NLog.SignalR.IntegrationTests.Hubs
{
    public class InProcessHubFixture
    {
        private IHubHost _host;
        public const string HubBaseUrl = "http://localhost:1234";

        [TestFixtureSetUp]
        public void Init()
        {
            StartHub();
        }

        protected void StartHub()
        {
            _host = new HubHost(HubBaseUrl);
            _host.Start();
        }

        protected void StopHub()
        {
            if (_host == null)
                return;

            _host.Stop();
            _host = null;
        }

        [TestFixtureTearDown]
        public void Dispose()
        {
            StopHub();
        }
    }
}