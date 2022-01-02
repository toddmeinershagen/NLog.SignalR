using System;
using NUnit.Framework;

namespace NLog.SignalR.IntegrationTests.Hubs
{
    public class InProcessHubFixture
    {
        private IHubHost _host;
        public static readonly string HubBaseUrl = "http://localhost:80/Temporary_Listen_Addresses/" + Guid.NewGuid().ToString("D") + "/";
        public static readonly string RestBaseUrl = "http://localhost:80/Temporary_Listen_Addresses/" + Guid.NewGuid().ToString("D") + "/";

        [OneTimeSetUp]
        public void Init()
        {
            StartHub();
        }

        protected void StartHub()
        {
            _host = new HubHost(HubBaseUrl, RestBaseUrl);
            _host.Start();
        }

        protected void StopHub()
        {
            if (_host == null)
                return;

            _host.Stop();
            _host = null;
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            StopHub();
        }
    }
}