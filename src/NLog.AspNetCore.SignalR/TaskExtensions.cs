using System.Threading.Tasks;

namespace NLog.SignalR
{
    internal static class TaskExtensions
    {
        public static Task AsTask(this Task task)
        {
            return task;
        }
    }
}
