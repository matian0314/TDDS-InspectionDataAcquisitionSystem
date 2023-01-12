using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Helpers
{
    public static class WPFApplicationExtensions
    {
        private static Process GetRunningInstance()
        {
            Process currentProcess = Process.GetCurrentProcess(); //获取当前进程
                                                                  //获取当前运行程序完全限定名 
            string currentFileName = Path.GetFileName(currentProcess.MainModule.FileName);
            //获取进程名为ProcessName的Process数组。 
            Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
            //遍历有相同进程名称正在运行的进程 
            foreach (Process process in processes)
            {
                if (Path.GetFileName(process.MainModule.FileName) == currentFileName)
                {
                    if (process.Id != currentProcess.Id) //根据进程ID排除当前进程 
                        return process;//返回已运行的进程实例 
                }
            }
            return null;
        }
        public static void ExitIfAnotherProcessIsRunning(this Application application)
        {
            Process p = GetRunningInstance();
            if (p != null)
            {
                application.Shutdown();
            }
        }
    }
}
