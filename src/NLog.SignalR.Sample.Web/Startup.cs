using Microsoft.Owin;
using NLog.SignalR.Sample.Web;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace NLog.SignalR.Sample.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}