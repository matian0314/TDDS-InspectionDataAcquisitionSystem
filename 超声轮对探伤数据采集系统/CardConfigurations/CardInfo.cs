using Eth;
using Helpers;
using MyLogger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using 超声轮对探伤数据采集系统.Eth;

namespace CardConfigurations
{
    public class CardInfo
    {
        private static readonly SubscribeLogger log = SubscribeLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
        /// <summary>
        /// Ip地址（形如192.168.1.20）
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// struct作为属性的话，无法修改结构体属性
        /// 如 Config.XXX = XXX会直接报错
        /// 故改为字段
        /// </summary>
        [JsonProperty]
        public gen_cfg_t Config;
        public int Index { get; set; }
        public string CardName { get; set; }
        public LineName LineName { get; set; }
        public CardInfo() { }
        public void SetGeneralConfig(gen_cfg_t config)
        {
            try
            {
                EthDev.set_gen_cfg(EthHelper.ToIp(Ip), ref config);
                //log.Debug($"EthDev.set_gen_cfg被调用,ip为{Ip},配置为{JsonConvert.SerializeObject(config)}");
                this.Config = config;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord());
            }
        }
        /// <summary>
        /// 将探头设置为自触发,
        /// </summary>
        /// <param name="config"></param>
        public void SetGeneralConfigSelfTrigger()
        {
            try
            {
                var config = this.Config;
                config.out_trig_ctrl = 1;
                config.measure_en = 1;
                EthDev.set_gen_cfg(EthHelper.ToIp(Ip), ref config);
                //log.Debug($"EthDev.SetGeneralConfigSelfTrigger被调用,ip为{Ip},配置为{JsonConvert.SerializeObject(config)}");
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord());
            }
        }
    }
}