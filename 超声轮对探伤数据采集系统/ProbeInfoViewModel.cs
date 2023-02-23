using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CardConfigurations;
using Eth;

namespace 超声轮对探伤数据采集系统
{
    public class ProbeInfoViewModel
    {
        public int Index { get; set; }
        public string ProbeName { get; set; }
        public string ChannelName { get; set; }
        public string Status { get; set; }
        public string ProbeType { get; set; }
        public double Db { get; set; }
        public double SoundPath { get; set; }
        public int PointCount { get; set; }
        public int AxleCount { get; set; }
        public string Key { get; set; }
        public string Ip { get; set; }
        public LineName LineName { get; set; }
        public SolidColorBrush ForegroundColor{get;set;}
        public int Channel { get; set; }
        public ProbeInfoViewModel(ProbeInfo probe, List<string> ConnectedIpList)
        {
            this.Index = probe.Index;
            this.ProbeName = probe.ProbeName;
            this.ChannelName = probe.ChannelName;
            this.ProbeType = probe.ProbeType;
            this.Db = probe.Db;
            this.SoundPath = probe.SoundPath;
            this.AxleCount = 0;
            this.PointCount = 0;
            this.Key = $"{probe.Ip}-{probe.Channel - 1}";
            this.Ip = probe.Ip;
            this.LineName = probe.LineName;
            this.Channel = probe.Channel;
            if (ConnectedIpList.Contains(probe.Ip))
            {
                this.Status = "已连接";
                this.ForegroundColor = Brushes.Black;
            }
            else
            {
                this.Status = "未连接";
                this.ForegroundColor = Brushes.Red;
            }
        }
        public ProbeInfoViewModel() { }
    }
}
