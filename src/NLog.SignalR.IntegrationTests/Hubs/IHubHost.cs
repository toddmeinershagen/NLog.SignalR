namespace NLog.SignalR.IntegrationTests.Hubs
{
    public interface IHubHost
    {
        void Start();
        void Stop();
    }
}