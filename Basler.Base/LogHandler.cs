using NLog;
namespace Basler.Base
{
    public class LogHandler
    {
        private static LogHandler _instance;
        public static LogHandler Instance => _instance ?? (_instance = new LogHandler());
        private LogHandler()
        {
            Log = LogManager.GetCurrentClassLogger();
        }
        public Logger Log { get; }
        public void Debug(string msg)
        {
            Log.Debug(msg);
        }
        public void Info(string msg)
        {
            Log.Info(msg);
        }
        public void Error(string msg)
        {
            Log.Error(msg);
        }
    }
}
