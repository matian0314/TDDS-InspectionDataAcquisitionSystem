using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;

namespace MyLogger
{
    public static class LogHelper
    {
        private static readonly ILog log;
        static LogHelper()
        {
            var directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Log4net");
            XmlConfigurator.Configure(new FileInfo(Path.Combine(directory, "log4net.config")));
            log = LogManager.GetLogger("LogHelper");
        }
        public static ILog GetLogger()
        {
            return log;
        }
    }
}
