using System;
using Microsoft.AspNet.SignalR.Client;

namespace NLog.SignalR
{
    public sealed class HubProxy : IDisposable
    {
        private HubConnection _connection;
        private IHubProxy _proxy;

        public void Log(LogEvent logEvent, string uri, string hubName, string methodName)
        {
            EnsureProxyExists(uri, hubName);
            _proxy?.Invoke(methodName, logEvent).ContinueWith(t => ProxyInvokeFailed(t), System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        }

        private static void ProxyInvokeFailed(System.Threading.Tasks.Task completedTask)
        {
            if (completedTask.Exception != null)
                NLog.Common.InternalLogger.Error(completedTask.Exception, "SignalR - Invoke Method Failure");
        }

        public void EnsureProxyExists(string uri, string hubName)
        {
            if (_proxy == null || _connection == null)
            {
                BeginNewConnection(uri, hubName);
            } 
            else if (_connection.State == ConnectionState.Disconnected)
            {
                StartExistingConnection(uri, hubName);
            }
        }

        private void BeginNewConnection(string uri, string hubName)
        {
            try
            {
                _connection = new HubConnection(uri);
                _proxy = _connection.CreateHubProxy(hubName);
                StartExistingConnection(uri, hubName);
                _proxy?.Invoke("Notify", _connection.ConnectionId);
            }
            catch (Exception ex)
            {
                NLog.Common.InternalLogger.Error(ex, "SignalR - Create Connection Failure. Uri={0}, HubName={1}", uri, hubName);
                _proxy = null;
                throw;
            }
        }

        private void StartExistingConnection(string uri, string hubName)
        {
            try
            {
                _connection.Start().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                NLog.Common.InternalLogger.Error(ex, "SignalR - Start Connection Failure. Uri={0}, HubName={1}", uri, hubName);
                _proxy = null;
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _connection?.Stop(TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                NLog.Common.InternalLogger.Error(ex, "SignalR - Stop Connection Failure");
            }
            finally
            {
                _connection = null;
                _proxy = null;
            }
        }

        public void Stop()
        {
            _connection?.Stop();
        }
    }
}
