using System;
using Microsoft.AspNet.SignalR.Client;

namespace NLog.SignalR
{
    public class HubProxy
    {
        public HubConnection Connection;
        private IHubProxy _proxy;

        public void Log(LogEvent logEvent, string uri, string hubName, string methodName)
        {
            EnsureProxyExists(uri, hubName);

            if (_proxy != null)
                _proxy.Invoke(methodName, logEvent).ContinueWith(t => ProxyInvokeFailed(t), System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        }

        private static void ProxyInvokeFailed(System.Threading.Tasks.Task completedTask)
        {
            if (completedTask.Exception != null)
                NLog.Common.InternalLogger.Error("SignalR - Invoke Method Failure. Exception={0}", completedTask.Exception);
        }

        public void EnsureProxyExists(string uri, string hubName)
        {
            if (_proxy == null || Connection == null)
            {
                BeginNewConnection(uri, hubName);
            } 
            else if (Connection.State == ConnectionState.Disconnected)
            {
                StartExistingConnection(uri, hubName);
            }
        }

        private void BeginNewConnection(string uri, string hubName)
        {
            try
            {
                Connection = new HubConnection(uri);
                _proxy = Connection.CreateHubProxy(hubName);
                StartExistingConnection(uri, hubName);
                _proxy?.Invoke("Notify", Connection.ConnectionId);
            }
            catch (Exception ex)
            {
                NLog.Common.InternalLogger.Error("SignalR - Create Connection Failure. Uri={0}, HubName={1}, Exception={2}", uri, hubName, ex);
                _proxy = null;
                throw;
            }
        }

        private void StartExistingConnection(string uri, string hubName)
        {
            try
            {
#if !NET40
                Connection.Start().ConfigureAwait(false).GetAwaiter().GetResult();
#else
                Connection.Start().Wait();
#endif
            }
            catch (Exception ex)
            {
                NLog.Common.InternalLogger.Error("SignalR - Start Connection Failure. Uri={0}, HubName={1}, Exception={2}", uri, hubName, ex);
                _proxy = null;
                throw;
            }
        }

    }
}
