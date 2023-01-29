using CardConfigurations;
using Eth;
using log4net;
using Newtonsoft.Json;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using 探伤报文;
using 探伤算法;
using 超声轮对探伤数据采集系统;

namespace 超声轮对探伤数据采集系统_佳木斯_
{
    public static class Program
    {
        public const string Version = "V1.0";
        public const string CompileTime = "2023年1月29日";
        [STAThread]
        public static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            ILog log = log4net.LogManager.GetLogger("Main");
            log.Info($"当前软件版本为{Version}");
            log.Info($"软件编译时间为2023年1月29日");
            //
            InspectionInfo info = new InspectionInfo();
            //DataRepo.RepoWrittenComplete.Invoke("2022年10月18日22时13分05秒");
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
