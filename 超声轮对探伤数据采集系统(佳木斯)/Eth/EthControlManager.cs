using Eth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardConfigurations;
using System.Threading;
using Helpers;
using _485通信._485协议;
using System.IO;

namespace 超声轮对探伤数据采集系统.Eth
{
    public class EthControlManager
    {
        //Public
        public CardConfigs Configs { get; set; }
        public List<string> ConnectedIpList { get; private set; } = new List<string>();
        public delegate void ConnectedIpListChangedHandler(List<string> ConnectedIpList);
        public event ConnectedIpListChangedHandler ConnectedIpListChanged;
        public event Action<int[], string, int> DataGenerated;
        //Private
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(nameof(MainWindow));
        private sbyte[,] PreviousIp = new sbyte[4, 100];
        private int ConnectedIpCount = 0;
        private volatile bool SendingMessageNow = false;
        private object lockConnectedIpListObj = new object();
        
        public EthControlManager()
        {
            try
            {
                Configs = CardConfigs.ReadFromFile();
                InitializeEthConttrolBox();
                UpdateConnectedIpListPeriodically();
                EthDev.IsReady = true;
                //在收到485开机信号时,开机;在收到485信号关机信号时,关机
                //Rs485Manager.ShutDownSignalReceived += OnReceiveShutdownSignal;
                //Rs485Manager.StartUpSignalReceived += OnReceiveStartUpSignal;
                EthDev.DataGenerated += OnEthDataGenerated;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord());
                throw;
            }

        }

        private void OnEthDataGenerated(ref byte[] points, string ip, int channel)
        {
            if (points.Length == 0 || points == null)
            {
                // No need to do anything if there are no samples.
                return;
            }
            if (points.Length != 500)
            {
                log.Info($"数组长度为{points.Length}，不为500");
                return;
            }
            int[] intPoints = new int[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                intPoints[i] = points[i];
            }
            DataGenerated?.Invoke(intPoints, ip, channel);
        }

        /// <summary>
        /// 初始化Eth板卡
        /// 设置回调函数
        /// </summary>
        public void InitializeEthConttrolBox()
        {
            if (RegexHelper.IsValidIpV4(Configs.LocalIp))
            {
                log.Info("正在连接控制箱");
                EthDev.set_deal_callback(EthDev.EthCallBack);
                EthDev.ut_eth_init(EthHelper.ToIp(Configs.LocalIp));
                EthDev.ut_eth_run();
                log.Debug("初始化完成");
            }
            else
            {
                throw new Exception("无效的本地IP地址");
            }
        }

        public void OnClosing()
        {
            DisableAllChannels();
            for (int i = 0; i < Configs.Cards.Count; i++)
            {
                Configs.Cards[i].Config.out_trig_ctrl = 0;
            }
            Configs.SaveConfigsToFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameof(CardConfigurations), "CardConfigs.cfg"));
        }
        public void ResetAllCardConfigs()
        {
            CardConfigs.CreateTestConfigFileForJiamusi();
            Configs = CardConfigs.ReadFromFile();
        }
        /// <summary>
        /// 开启一个周期更新已连接IP地址的线程
        /// 如果已连接的IP地址有变化，触发OnConnectedIpListChanged事件
        /// </summary>
        public void UpdateConnectedIpListPeriodically()
        {
            Task.Run(() =>
            {

                while (true)
                {
                    try
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(3));

                        //如果正在给各个探头发送配置信息
                        //则此次不进行操作
                        if (SendingMessageNow)
                        {
                            continue;
                        }

                        sbyte[,] currentIp = new sbyte[4, 100];
                        EthDev.get_connected_ip(currentIp, ref ConnectedIpCount);
                        if (!currentIp.IsArrayEqual(PreviousIp, 4, 100))
                        {
                            log.Debug($"{nameof(UpdateConnectedIpListPeriodically)}:连接的Ip地址有变化,当前连接数为{ConnectedIpCount}");
                            PreviousIp = currentIp;
                            lock (lockConnectedIpListObj)
                            {
                                ConnectedIpList = new List<string>();
                                for (int i = 0; i < ConnectedIpCount; i++)
                                {
                                    ConnectedIpList.Add(EthHelper.IpToString(EthHelper.GetIpFromArray(currentIp, i)));
                                }
                                foreach (var ip in ConnectedIpList)
                                {
                                    var card = Configs.Cards.Where(c => c.Ip == ip).FirstOrDefault();

                                    if (card == null)
                                    {
                                        log.Error($"{nameof(UpdateConnectedIpListPeriodically)}：未知的Ip{ip}连接");
                                        continue;
                                    }
                                    var genConfig = card.Config;
                                    genConfig.measure_en = 0;
                                    card.SetGeneralConfig(genConfig);
                                    foreach (var probe in Configs.Probes.Where(p => p.Ip == ip))
                                    {
                                        var chnConfig = probe.Config;
                                        chnConfig.trig_en = 0;
                                        probe.SetChannelConfig(chnConfig);
                                    }
                                }
                            }
                            log.Debug("ConnectedIpListChanged触发");
                            ConnectedIpListChanged?.Invoke(ConnectedIpList);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.ToRecord());
                    }

                }
            });
        }

        public void EnableAllChannels()
        {
            this.SendingMessageNow = true;
            foreach (string ip in this.ConnectedIpList)
            {
                EnableCardAndChannels(ip);
            }
            this.SendingMessageNow = false;
        }
        public void EnableAllChannelSelfTrigger()
        {
            this.SendingMessageNow = true;
            foreach (string ip in this.ConnectedIpList)
            {

                var card = Configs.Cards.First(c => c.Ip == ip);
                card.SetGeneralConfigSelfTrigger();
                foreach (var probe in Configs.Probes.Where(p => p.Ip == ip))
                {
                    probe.SetChannelSelfTrigger();
                }
            }
            this.SendingMessageNow = false;
        }
        public void DisableAllChannelSelfTrigger()
        {
            this.SendingMessageNow = true;
            foreach (var card in Configs.Cards)
            {
                card.Config.measure_en = 0;
                card.Config.out_trig_ctrl = 0;
                card.SetGeneralConfig(card.Config);
            }
            foreach (var probe in Configs.Probes)
            {
                probe.Config.trig_en = 0;
                probe.SetChannelConfig(probe.Config);
            }
            this.SendingMessageNow = false;
        }
        public void DisableAllChannels()
        {
            this.SendingMessageNow = true;
            foreach (string ip in this.ConnectedIpList)
            {
                DisableCardAndChannels(ip);
            }
            //缓冲5s，等待消息全部发送完成
            Thread.Sleep(TimeSpan.FromSeconds(5));
            this.SendingMessageNow = false;
        }
        private void EnableCardAndChannels(string ip)
        {
            foreach (var card in Configs.Cards.Where(c => c.Ip == ip))
            {
                var config = card.Config;
                config.measure_en = 1;
                card.SetGeneralConfig(config);
                card.Config = config;
            }
            foreach (var probe in Configs.Probes.Where(p => p.Ip == ip))
            {
                var config = probe.Config;
                config.trig_en = 1;
                probe.SetChannelConfig(config);
                probe.Config = config;
            }
        }
        private void DisableCardAndChannels(string ip)
        {
            foreach (var card in Configs.Cards.Where(c => c.Ip == ip))
            {
                var config = card.Config;
                config.measure_en = 0;
                card.SetGeneralConfig(config);
                card.Config = config;
            }
            foreach (var probe in Configs.Probes.Where(p => p.Ip == ip))
            {
                var config = probe.Config;
                config.trig_en = 0;
                probe.SetChannelConfig(config);
                probe.Config = config;
            }

        }
        private void OnReceiveShutdownSignal(object signal)
        {
            
            if (signal is ShutDownSignal)
            {
                var shutDownsignal = signal as ShutDownSignal;
                DisableAllChannels();
                DataRepo.WriteToFile(this.Configs);
            }
            else
            {
                log.Error("收到错误的关机信号");
                return;
            }
        }
        private void OnReceiveStartUpSignal(object signal)
        {
            if (signal is StartUpSignal)
            {
                var shutDownsignal = signal as StartUpSignal;
                log.Info("开启所有通道");
                EnableAllChannels();
            }
            else
            {
                log.Error("收到错误的开机信号");
                return;
            }
        }
        public void RestartConnection()
        {
            log.Info("正在连接控制箱");
            EthDev.set_deal_callback(EthDev.EthCallBack);
            EthDev.ut_eth_init(EthHelper.ToIp(Configs.LocalIp));
            EthDev.ut_eth_run();
            log.Debug("初始化完成");
        }
    }
}
