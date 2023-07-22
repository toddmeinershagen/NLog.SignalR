using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace NLog.SignalR
{
    public sealed class HubProxy : IDisposable
    {
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private HubConnection _connection;
        private Task _proxyTask;

        public void Log(LogEvent logEvent, string uri, string hubName, string methodName)
        {
            var connection = EnsureConnectionExists(uri);

            var cancellationToken = _cancellationToken;
            _proxyTask = _proxyTask.ContinueWith(async task =>
            {
                if (task.Exception != null)
                {
                    NLog.Common.InternalLogger.Error(task.Exception, "SignalR - Invoke Method Failure");
                }

                try
                {
                    await connection.InvokeCoreAsync(methodName, new[] { logEvent }, cancellationToken.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    NLog.Common.InternalLogger.Error(ex, "SignalR - Invoke Method Failure");
                }
            }, cancellationToken.Token);
        }

        public HubConnection EnsureConnectionExists(string uri)
        {
            if (_proxyTask != null && _connection?.State == HubConnectionState.Disconnected)
            {
                try
                {
                    _proxyTask = _connection.StartAsync(_cancellationToken.Token);
                }
                catch (Exception ex)
                {
                    NLog.Common.InternalLogger.Error(ex, "SignalR - Start Connection Failure. Uri={0}", uri);
                    _proxyTask = null;
                }
            }

            if (_proxyTask == null || _connection == null)
            {
                try
                {
                    var connection = CreateHubConnection(uri, _cancellationToken);
                    _proxyTask = connection.StartAsync(_cancellationToken.Token);
                    _connection = connection;
                }
                catch (Exception ex)
                {
                    NLog.Common.InternalLogger.Error(ex, "SignalR - Start Connection Failure. Uri={0}", uri);
                    _proxyTask = null;
                    throw;
                }
            }

            return _connection;
        }

        private static HubConnection CreateHubConnection(string uri, CancellationTokenSource cancellationToken)
        {
            HubConnection connection = new HubConnectionBuilder().WithUrl(uri).Build();
            connection.Closed += (error) =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    NLog.Common.InternalLogger.Debug("SignalR - Connection closed. Reason: {0}", error);
                }
                return Task.CompletedTask;
            };
            return connection;
        }

        public void Flush(NLog.Common.AsyncContinuation asyncContinuation)
        {
            (_proxyTask ?? Task.CompletedTask).ContinueWith(task => asyncContinuation.Invoke(task.Exception));
        }

        public void Stop()
        {
            _connection?.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            try
            {
                if (!_cancellationToken.IsCancellationRequested)
                    _cancellationToken.CancelAfter(2000);
                _connection?.DisposeAsync().AsTask().Wait(_cancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                // Connection has been cancelled
            }
            catch (Exception ex)
            {
                NLog.Common.InternalLogger.Error(ex, "SignalR - Stop Connection Failure");
            }
            finally
            {
                _connection = null;
                _proxyTask = null;
            }
        }
    }
}
