using Arction.Wpf.SignalProcessing;
using Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
namespace 超声轮对探伤数据采集系统
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public log4net.ILog log = log4net.LogManager.GetLogger("App");
        public App()
        {
            try
            {
                //LightningChart的LiscenseKey
                string deploymentKey = @"lgCAAN++8Js9AdgBJABVcGRhdGVhYmxlVGlsbD0yMDIzLTAxLTA0I1JldmlzaW9uPTAI/z8AAAAA/AFIMQtCCbhOIsiWJQtM4DNg7+Kye60RurJD1sBXMgDNmSa/nl1t2H3ImRIQw0uNQqadm0W2jqNpEx+6J42+Yzvv8NiZ0rd42tSg4v2igA5lzrPC0Pi25gvDhufUSfgMZk32XeTbM3tnHdbLDqHfYsWo0Q3CYcesWaczGvRsqgikzzWVekG3b+e+LK7ZIZugoLv001lypruYNUljoNzupFO3+52XmkTpynNc1g9gNPR7fh2YVSDgs++0mC2rohX5ign+nkIW6Tscx2T6a4Ew3dEQ08oOsHERL4yXVyPV46siPsw+TAHwKd38kY7CJwftyT4MLGo3hAox9TDvTnQUr0UsS4OXk5iUoP1tftVwj0tkT8NjvHWPzvCWSk6Lsd1xc6GlpVERyH/mIHbnqDMO16cs473tDDHlPMzkzXbDzPPx0emwgAzi8xqZFLMEsF7/AXEznl/CV3RFp6fh2wt4kj2vq7x6Hp/8YjfKp88sg3gNzVWSk2D8kZi8n7rskiJD/NY=";
                Arction.Wpf.Charting.LightningChart.SetDeploymentKey(deploymentKey);
                //为其他Arction组件部署秘钥
                SignalGenerator.SetDeploymentKey(deploymentKey);
                AudioInput.SetDeploymentKey(deploymentKey);
                AudioOutput.SetDeploymentKey(deploymentKey);
                SpectrumCalculator.SetDeploymentKey(deploymentKey);
                SignalReader.SetDeploymentKey(deploymentKey);
                //报错 ： 32位程序，无法调用64的api
                //this.ExitIfAnotherProcessIsRunning();
                //wpf捕获异常而不崩溃退出
                DispatcherUnhandledException += App_DispatcherUnhandledException;
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord());
                throw;
            }

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error($"进入{nameof(CurrentDomain_UnhandledException)}处理");
            log.Error(((Exception)e.ExceptionObject).ToRecord());
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            log.Error($"进入{nameof(TaskScheduler_UnobservedTaskException)}处理");
            log.Error(e.Exception.ToRecord());
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            log.Error($"进入{nameof(App_DispatcherUnhandledException)}处理");
            log.Error(e.Exception.ToRecord());
            if (e.Exception is DllNotFoundException)
            {//Dll未正确加载(一般为eth.dll)的异常，程序退出，说明环境没配置好
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }

        }
    }
}
