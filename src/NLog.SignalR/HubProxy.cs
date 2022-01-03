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
            if (_proxy != null && _connection?.State == ConnectionState.Disconnected)
            {
                if (!StartExistingConnection(uri, hubName))
                {
                    _proxy = null;
                }
            }

            if (_proxy == null || _connection == null)
            {
                BeginNewConnection(uri, hubName);
            } 
        }

        private void BeginNewConnection(string uri, string hubName)
        {
            try
            {
                var connection = new HubConnection(uri);
                connection.Error += (ex) =>
                {
                    NLog.Common.InternalLogger.Error(ex, "SignalR - Connection Failure. Uri={0}, HubName={1}", uri, hubName);
                };

                _proxy = connection.CreateHubProxy(hubName);
                _connection = connection;
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

        private bool StartExistingConnection(string uri, string hubName)
        {
            try
            {
                _connection.Start().ConfigureAwait(false).GetAwaiter().GetResult();
                if (_connection.State != ConnectionState.Connected)
                {
                    NLog.Common.InternalLogger.Error("SignalR - Start Connection Failure. Uri={0}, HubName={1}", uri, hubName);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                NLog.Common.InternalLogger.Error(ex, "SignalR - Start Connection Failure. Uri={0}, HubName={1}", uri, hubName);
                _proxy = null;
                return false;
            }
        }

        public void Flush(NLog.Common.AsyncContinuation asyncContinuation)
        {
            asyncContinuation(null);
        }

        public void Stop()
        {
            _connection?.Stop();
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
    }
}
