using CardConfigurations;
using Eth;
using log4net;
using Microsoft.Scripting.Utils;
using MyLogger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tools;
using 探伤报文;
using 探伤算法;

namespace 超声轮对探伤数据采集系统
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            SubscribeLogger log = SubscribeLogger.GetLogger("Main");
            var assembly = Assembly.GetExecutingAssembly();
            log.Info($"软件版本: {assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version}");
            log.Info($"{assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description}");
            try
            {
                App app = new App();
                app.InitializeComponent();
                app.Run();
                
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord(RecordOptions.All));
                throw;
            }

        }
    }
}
