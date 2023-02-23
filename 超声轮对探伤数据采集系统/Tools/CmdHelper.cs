using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class CmdHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void RunPythonScript(string interpreter, string fileName, params string[] args)
        {
            RunPythonScript(interpreter, fileName, null, null, args);
        }
        public static void RunPythonScript(string interpreter, string fileName, DataReceivedEventHandler dataHandler = null, DataReceivedEventHandler errorEventHandler = null, params string[] args)
        {
            Process p = new Process();
            p.StartInfo.FileName = interpreter;
            p.StartInfo.Arguments = fileName + " " + string.Join(" ", args);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            p.OutputDataReceived += dataHandler ?? Default_OutputDataReceived;
            p.ErrorDataReceived += errorEventHandler ?? Default_ErrorDataReceived;

            p.Start();
            p.BeginOutputReadLine();
            p.WaitForExit();
        }
        public static void RunCmd(string command, DataReceivedEventHandler dataHandler = null, DataReceivedEventHandler errorEventHandler = null, params string[] args)
        {
            Process p = new Process();
            p.StartInfo.FileName = command;
            p.StartInfo.Arguments = string.Join(" ", args);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            p.OutputDataReceived += dataHandler ?? Default_OutputDataReceived;
            p.ErrorDataReceived += errorEventHandler ?? Default_ErrorDataReceived;

            p.Start();
            p.BeginOutputReadLine();
            p.WaitForExit();
        }
        private static void Default_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                log.Error(e.Data);
            }
        }

        private static void Default_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                log.Info(e.Data);
            }
        }
    }
}
