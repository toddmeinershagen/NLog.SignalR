using System;

namespace NLog.SignalR.IntegrationTests.Hubs
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HubHost(args[0]);
            host.Start();

            Console.WriteLine("Service is listening...");
            Console.ReadKey();
        }
    }
}
