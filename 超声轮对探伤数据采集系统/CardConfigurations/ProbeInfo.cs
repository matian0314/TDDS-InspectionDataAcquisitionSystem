using Eth;
using Helpers;
using Newtonsoft.Json;
using System;
using 超声轮对探伤数据采集系统.Eth;

namespace CardConfigurations
{
    public class ProbeInfo
    {
        private log4net.ILog log = log4net.LogManager.GetLogger(nameof(ProbeInfo));
        /// <summary>
        /// 在YW/YN/ZN/ZW中的探头编号
        /// </summary>
        public int Index { get; set; }
        public string Ip { get; set; }
        /// <summary>
        /// 属于这块板卡的第几个通道 从1开始
        /// </summary>
        public int Channel { get; set; }
        public string ProbeName { get; set; }
        public string ChannelName { get; set; }
        public string CardName { get; set; }
        /// <summary>
        /// 分为 直探头 斜探头 斜探头-轮缘
        /// </summary>
        public string ProbeType { get; set; }
        /// <summary>
        /// 直探头 Z 斜探头 X 斜探头-轮缘 Y
        /// </summary>
        public string ProbeTypeCode { get; set; }
        [JsonIgnore]
        public double Db { 
            get
            {
                return Config.db;
            }
        }
        [JsonIgnore]
        public double SoundPath 
        { 
            get
            {
                return chan_cfg_t.GetPointInterval(this.ProbeType, Config) * 500;
            }
        }
        /// <summary>
        /// struct作为属性的话，无法修改结构体属性
        /// 如 Config.XXX = XXX会直接报错
        /// 故改为字段
        /// </summary>
        [JsonProperty]
        public chan_cfg_t Config;
        public LineName LineName { get; set; }
        public ProbeInfo() { }
        public void SetChannelConfig(chan_cfg_t config)
        {
            try
            {
                EthDev.set_chan_cfg(EthHelper.ToIp(Ip), ref config);
                //log.Debug($"EthDev.set_chan_cfg被调用,Ip为{Ip},通道为{config.dest_chan},配置为{JsonConvert.SerializeObject(config)}");
                this.Config = config;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord());
            }
        }
        public void SetChannelSelfTrigger()
        {
            var config = this.Config;
            config.trig_en = 1;
            EthDev.set_chan_cfg(EthHelper.ToIp(Ip), ref config);
            //log.Debug($"SetChannelSelfTrigger被调用,Ip为{Ip},通道为{config.dest_chan},使能位为{config.trig_en}");
            //log.Debug($"详细数据为{JsonConvert.SerializeObject(config)}");
        }
    }
}