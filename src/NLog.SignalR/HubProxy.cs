using System;
using Microsoft.AspNet.SignalR.Client;

namespace NLog.SignalR
{
    public class HubProxy
    {
        private readonly SignalRTarget _target;
        public HubConnection Connection;
        private IHubProxy _proxy;

        public HubProxy(SignalRTarget target)
        {
            _target = target;
        }

        public void Log(LogEvent logEvent)
        {
            EnsureProxyExists();

            if (_proxy != null)
                _proxy.Invoke(_target.MethodName, logEvent);
        }

        public void EnsureProxyExists()
        {
            if (_proxy == null || Connection == null)
            {
                BeginNewConnection();
            } 

            else if (Connection.State == ConnectionState.Disconnected)
            {
                StartExistingConnection();
            }
        }

        private void BeginNewConnection()
        {
            try
            {
                Connection = new HubConnection(_target.Uri);
                _proxy = Connection.CreateHubProxy(_target.HubName);
                Connection.Start().Wait();

                _proxy.Invoke("Notify", Connection.ConnectionId);
            }
            catch (Exception)
            {
                _proxy = null;
            }
        }

        private void StartExistingConnection()
        {
            try
            {
                Connection.Start().Wait();
            }
            catch (Exception)
            {
                _proxy = null;
            }
        }

    }
}
