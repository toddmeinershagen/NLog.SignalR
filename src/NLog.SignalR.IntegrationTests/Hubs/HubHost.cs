using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;

namespace NLog.SignalR.IntegrationTests.Hubs
{
    public class HubHost : IHubHost
    {
        private bool _disposed;
        private readonly Uri _baseUri;
        private IDisposable _webApp;

        public HubHost(string baseUrl)
        {
            _baseUri = new Uri(baseUrl);
        }

        public void Start()
        {
            var url = string.Format("http://+:{0}", _baseUri.Port);
            _webApp = WebApp.Start<Startup>(url);
            _disposed = false;
        }

        public void Stop()
        {
            if (_disposed)
                return;

            if (_webApp == null)
                return;
                
            _webApp.Dispose();
            _webApp = null;
            _disposed = true;
        }

        internal class LoggingHub : Hub<ILoggingHub>
        {
            public void Log(LogEvent logEvent)
            {
                Test.Current.SignalRLogEvents.Add(logEvent);

                var client = new HttpClient{BaseAddress = new Uri(OutOfProcessHubFixture.RestBaseUrl)};
                var response = client.PostAsync("SignalRLogEvents", logEvent, new JsonMediaTypeFormatter()).Result;

                Clients.All.Log(logEvent);
            }
        }

        internal class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                app.MapSignalR();
            }
        }
    }
}