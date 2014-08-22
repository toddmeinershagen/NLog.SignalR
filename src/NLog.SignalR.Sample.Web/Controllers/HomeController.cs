using System;
using System.Web.Mvc;

namespace NLog.SignalR.Sample.Web.Controllers
{
    public class HomeController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Random _generator = new Random();

        public ActionResult Index()
        {
            Logger.Info("Index()");
            return View();
        }

        public JsonResult AddLogEntry()
        {
            var index = _generator.Next(0, Messages.Length);
            var message = Messages[index];
            Logger.Log(message.Level, message.Message);

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public LogEventInfo[] Messages =
        {
            new LogEventInfo{Level = LogLevel.Info, Message = "Hello, world!"},
            new LogEventInfo{Level = LogLevel.Error, Message = "There is a server issue."},
            new LogEventInfo{Level = LogLevel.Trace, Message = "You have entered the AddLogEntry() method."}
        };
    }
}
