using Eth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 超声轮对探伤数据采集系统;

namespace CardConfigurations
{
    public class CardConfigs
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(nameof(MainWindow));
        //只能由读取的方式生成
        private CardConfigs() { }
        public static CardConfigs ReadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                CreateTestConfigFileForJiamusi();
            }
            string configString;
            using (StreamReader sr = File.OpenText(path))
            {
                configString = sr.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<CardConfigs>(configString);
        }
        public static CardConfigs ReadFromFile()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameof(CardConfigurations),"CardConfigs.cfg");
            return ReadFromFile(path);
        }
        public void SaveConfigsToFile(string path)
        {
            string info = JsonConvert.SerializeObject(this);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using(StreamWriter sw = File.CreateText(path))
            {
                sw.Write(info);
                sw.Flush();
            }
        }
        public void SaveConfigsToFile()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameof(CardConfigurations), "CardConfigs.cfg");
            SaveConfigsToFile(path);
        }
        /// <summary>
        /// YN/YW/ZN/ZW有多少个探头
        /// </summary>
        public int ProbePerSide { get; set; }
        /// <summary>
        /// YN/YW/ZN/ZW有多少块板卡
        /// </summary>
        public int CardPerSide { get; set; }
        /// <summary>
        /// 一块板卡上有多少块探头
        /// </summary>
        public int ProbePerCard { get; set; }
        public string LocalIp { get; set; }
        public List<CardInfo> Cards { get; set; } = new List<CardInfo>();
        public List<ProbeInfo> Probes { get; set; } = new List<ProbeInfo>();
        /// <summary>
        /// 根据齐齐哈尔探头和板卡编号协议
        /// 编写默认的配置信息
        /// </summary>
        public static void CreateTestConfigFileForQiqihaer()
        {
            CardConfigs configs = new CardConfigs()
            {
                CardPerSide = 10,
                ProbePerCard = 16,
                ProbePerSide = 136,
                LocalIp = ConfigurationManager.AppSettings["LocalIp"],
            };
            //YW
            for (int i = 1; i <= 10; i++)
            {
                CardInfo card = new CardInfo()
                {
                    CardName = $"YWK-{i:00}",
                    Index = i,
                    Ip = $"192.168.1.{19 + i:00}",
                    Config = gen_cfg_t.Create(16),
                    LineName = LineName.YW
                };
                configs.Cards.Add(card);
            }
            for (int i = 1; i <= 160; i++)
            {
                string type;
                switch (i % 4)
                {
                    case 1:
                        type = "X";
                        break;
                    case 2:
                    case 0:
                    default:
                        type = "Z";
                        break;
                    case 3:
                        type = "Y";
                        break;
                }
                if (i <= 80)
                {
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"YWK-{(i - 1) % 5 + 1:00}-{(i - 1) / 5 + 1:00}{type}",
                        Channel = (i - 1) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 1) / 5, type),
                        Ip = $"192.168.1.{19 + (i - 1) % 5 + 1:00}",
                        ProbeName = $"YWT-{i:000}{type}",
                        CardName = $"YWK-{(i - 1) % 5 + 1:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.YW
                    };
                    configs.Probes.Add(probe);
                }
                else
                {
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"YWK-{(i - 1) % 5 + 6:00}-{(i - 81) / 5 + 1:00}{type}",
                        Channel = (i - 81) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 81) / 5, type),
                        Ip = $"192.168.1.{19 + (i - 1) % 5 + 6:00}",
                        ProbeName = $"YWT-{i:000}{type}",
                        CardName = $"YWK-{(i - 1) % 5 + 6:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.YW
                    };
                    configs.Probes.Add(probe);
                }
            }
            //YN
            for (int i = 1; i <= 10; i++)
            {
                CardInfo card = new CardInfo()
                {
                    CardName = $"YNK-{i:00}",
                    Index = i,
                    Ip = $"192.168.1.{29 + i:00}",
                    Config = gen_cfg_t.Create(16),
                    LineName = LineName.YN
                };
                configs.Cards.Add(card);
            }
            for (int i = 1; i <= 160; i++)
            {
                string type;
                switch (i % 4)
                {
                    case 0:
                        type = "X";
                        break;
                    case 1:
                    case 3:
                    default:
                        type = "Z";
                        break;
                    case 2:
                        type = "Y";
                        break;
                }
                if (i <= 80)
                {
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"YNK-{(i - 1) % 5 + 1:00}-{(i - 1) / 5 + 1:00}{type}",
                        Channel = (i - 1) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 1) / 5, type),
                        Ip = $"192.168.1.{29 + (i - 1) % 5 + 1:00}",
                        ProbeName = $"YNT-{i:000}{type}",
                        CardName = $"YNK-{(i - 1) % 5 + 1:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.YN
                    };
                    configs.Probes.Add(probe);
                }
                else
                {
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"YNK-{(i - 1) % 5 + 6:00}-{(i - 81) / 5 + 1:00}{type}",
                        Channel = (i - 81) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 81) / 5, type),
                        Ip = $"192.168.1.{29 + (i - 1) % 5 + 6:00}",
                        ProbeName = $"YNT-{i:000}{type}",
                        CardName = $"YNK-{(i - 1) % 5 + 6:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.YN
                    };
                    configs.Probes.Add(probe);
                }
            }
            //ZN
            for (int i = 1; i <= 10; i++)
            {
                CardInfo card = new CardInfo()
                {
                    CardName = $"ZNK-{i:00}",
                    Index = i,
                    Ip = $"192.168.1.{39 + i:00}",
                    Config = gen_cfg_t.Create(16),
                    LineName = LineName.ZN
                };
                configs.Cards.Add(card);
            }
            for (int i = 1; i <= 160; i++)
            {
                string type;
                switch (i % 4)
                {
                    case 0:
                        type = "X";
                        break;
                    case 1:
                    case 3:
                    default:
                        type = "Z";
                        break;
                    case 2:
                        type = "Y";
                        break;
                }
                if (i <= 80)
                {
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"ZNK-{(i - 1) % 5 + 1:00}-{(i - 1) / 5 + 1:00}{type}",
                        Channel = (i - 1) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 1) / 5, type),
                        Ip = $"192.168.1.{39 + (i - 1) % 5 + 1:00}",
                        ProbeName = $"ZNT-{i:000}{type}",
                        CardName = $"ZNK-{(i - 1) % 5 + 1:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.ZN
                    };
                    configs.Probes.Add(probe);
                }
                else
                {
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"ZNK-{(i - 1) % 5 + 6:00}-{(i - 81) / 5 + 1:00}{type}",
                        Channel = (i - 81) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 81) / 5, type),
                        Ip = $"192.168.1.{39 + (i - 1) % 5 + 6:00}",
                        ProbeName = $"ZNT-{i:000}{type}",
                        CardName = $"ZNK-{(i - 1) % 5 + 6:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.ZN
                    };
                    configs.Probes.Add(probe);
                }
            }
            //ZW
            for (int i = 1; i <= 10; i++)
            {
                CardInfo card = new CardInfo()
                {
                    CardName = $"ZWK-{i:00}",
                    Index = i,
                    Ip = $"192.168.1.{49 + i:00}",
                    Config = gen_cfg_t.Create(16),
                    LineName = LineName.ZW
                };
                configs.Cards.Add(card);
            }
            for (int i = 1; i <= 160; i++)
            {
                string type;
                switch (i % 4)
                {
                    case 1:
                        type = "X";
                        break;
                    case 2:
                    case 0:
                    default:
                        type = "Z";
                        break;
                    case 3:
                        type = "Y";
                        break;
                }
                if (i <= 80)
                {
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"ZWK-{(i - 1) % 5 + 1:00}-{(i - 1) / 5 + 1:00}{type}",
                        Channel = (i - 1) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 1) / 5, type),
                        Ip = $"192.168.1.{49 + (i - 1) % 5 + 1:00}",
                        ProbeName = $"ZWT-{i:000}{type}",
                        CardName = $"ZWK-{(i - 1) % 5 + 1:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.ZW
                    };
                    configs.Probes.Add(probe);
                }
                else
                {
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"ZWK-{(i - 1) % 5 + 6:00}-{(i - 81) / 5 + 1:00}{type}",
                        Channel = (i - 81) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 81) / 5, type),
                        Ip = $"192.168.1.{49 + (i - 1) % 5 + 6:00}",
                        ProbeName = $"ZWT-{i:000}{type}",
                        CardName = $"ZWK-{(i - 1) % 5 + 6:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.ZW
                    };
                    configs.Probes.Add(probe);
                }
            }
            foreach (var probe in configs.Probes)
            {
                switch (probe.ProbeTypeCode)
                {
                    default:
                        throw new Exception($"无效的ProbeTypeCode值{probe.ProbeTypeCode}");
                    case "Z":
                        probe.ProbeType = "直探头";
                        break;
                    case "Y":
                        probe.ProbeType = "斜探头-轮缘";
                        break;
                    case "X":
                        probe.ProbeType = "斜探头";
                        break;
                }
            }
            string info = JsonConvert.SerializeObject(configs);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameof(CardConfigurations), "CardConfigs.cfg");
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameof(CardConfigurations)));
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(info);
                sw.Flush();
            }
        }
        public static void CreateTestConfigFileForJiamusi()
        {
            log.Info("生成配置文件");
            CardConfigs configs = new CardConfigs()
            {
                CardPerSide = 10,
                ProbePerCard = 16,
                ProbePerSide = 160,
                LocalIp = ConfigurationManager.AppSettings["LocalIp"],
            };
            //YW
            for (int i = 1; i <= 10; i++)
            {
                CardInfo card = new CardInfo()
                {
                    CardName = $"YWK-{i:00}",
                    Index = i,
                    Ip = $"192.168.1.{19 + i:00}",
                    Config = gen_cfg_t.Create(16),
                    LineName = LineName.YW
                };
                configs.Cards.Add(card);
            }
            for (int i = 1; i <= 160; i++)
            {
                string type;
                switch (i % 4)
                {
                    case 1:
                        type = "X";
                        break;
                    case 2:
                    case 0:
                    default:
                        type = "Z";
                        break;
                    case 3:
                        type = "Y";
                        break;
                }
                if (i <= 80)
                {
                    if (i > 68) continue;
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"YWK-{(i - 1) % 5 + 1:00}-{(i - 1) / 5 + 1:00}{type}",
                        Channel = (i - 1) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 1) / 5, type),
                        Ip = $"192.168.1.{19 + (i - 1) % 5 + 1:00}",
                        ProbeName = $"YWT-{i:000}{type}",
                        CardName = $"YWK-{(i - 1) % 5 + 1:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.YW
                    };
                    configs.Probes.Add(probe);
                }
                else
                {
                    if (i > 148) continue;
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"YWK-{(i - 1) % 5 + 6:00}-{(i - 81) / 5 + 1:00}{type}",
                        Channel = (i - 81) / 5 + 1,
                        Index = i - 12,
                        Config = chan_cfg_t.Create((i - 81) / 5, type),
                        Ip = $"192.168.1.{19 + (i - 1) % 5 + 6:00}",
                        ProbeName = $"YWT-{i - 12:000}{type}",
                        CardName = $"YWK-{(i - 1) % 5 + 6:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.YW
                    };
                    configs.Probes.Add(probe);
                }
            }
            //YN
            for (int i = 1; i <= 10; i++)
            {
                CardInfo card = new CardInfo()
                {
                    CardName = $"YNK-{i:00}",
                    Index = i,
                    Ip = $"192.168.1.{29 + i:00}",
                    Config = gen_cfg_t.Create(16),
                    LineName = LineName.YN
                };
                configs.Cards.Add(card);
            }
            for (int i = 1; i <= 160; i++)
            {
                string type;
                switch (i % 4)
                {
                    case 0:
                        type = "X";
                        break;
                    case 1:
                    case 3:
                    default:
                        type = "Z";
                        break;
                    case 2:
                        type = "Y";
                        break;
                }
                if (i <= 80)
                {
                    if (i > 68) continue;
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"YNK-{(i - 1) % 5 + 1:00}-{(i - 1) / 5 + 1:00}{type}",
                        Channel = (i - 1) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 1) / 5, type),
                        Ip = $"192.168.1.{29 + (i - 1) % 5 + 1:00}",
                        ProbeName = $"YNT-{i:000}{type}",
                        CardName = $"YNK-{(i - 1) % 5 + 1:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.YN
                    };
                    configs.Probes.Add(probe);
                }
                else
                {
                    if (i > 148) continue;
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"YNK-{(i - 1) % 5 + 6:00}-{(i - 81) / 5 + 1:00}{type}",
                        Channel = (i - 81) / 5 + 1,
                        Index = i - 12,
                        Config = chan_cfg_t.Create((i - 81) / 5, type),
                        Ip = $"192.168.1.{29 + (i - 1) % 5 + 6:00}",
                        ProbeName = $"YNT-{i - 12:000}{type}",
                        CardName = $"YNK-{(i - 1) % 5 + 6:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.YN
                    };
                    configs.Probes.Add(probe);
                }
            }
            //ZN
            for (int i = 1; i <= 10; i++)
            {
                CardInfo card = new CardInfo()
                {
                    CardName = $"ZNK-{i:00}",
                    Index = i,
                    Ip = $"192.168.1.{39 + i:00}",
                    Config = gen_cfg_t.Create(16),
                    LineName = LineName.ZN
                };
                configs.Cards.Add(card);
            }
            for (int i = 1; i <= 160; i++)
            {
                string type;
                switch (i % 4)
                {
                    case 0:
                        type = "X";
                        break;
                    case 1:
                    case 3:
                    default:
                        type = "Z";
                        break;
                    case 2:
                        type = "Y";
                        break;
                }
                if (i <= 80)
                {
                    if (i > 68) continue;
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"ZNK-{(i - 1) % 5 + 1:00}-{(i - 1) / 5 + 1:00}{type}",
                        Channel = (i - 1) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 1) / 5, type),
                        Ip = $"192.168.1.{39 + (i - 1) % 5 + 1:00}",
                        ProbeName = $"ZNT-{i:000}{type}",
                        CardName = $"ZNK-{(i - 1) % 5 + 1:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.ZN
                    };
                    configs.Probes.Add(probe);
                }
                else
                {
                    if (i > 148) continue;
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"ZNK-{(i - 1) % 5 + 6:00}-{(i - 81) / 5 + 1:00}{type}",
                        Channel = (i - 81) / 5 + 1,
                        Index = i - 12,
                        Config = chan_cfg_t.Create((i - 81) / 5, type),
                        Ip = $"192.168.1.{39 + (i - 1) % 5 + 6:00}",
                        ProbeName = $"ZNT-{i - 12:000}{type}",
                        CardName = $"ZNK-{(i - 1) % 5 + 6:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.ZN
                    };
                    configs.Probes.Add(probe);
                }
            }
            //ZW
            for (int i = 1; i <= 10; i++)
            {
                CardInfo card = new CardInfo()
                {
                    CardName = $"ZWK-{i:00}",
                    Index = i,
                    Ip = $"192.168.1.{49 + i:00}",
                    Config = gen_cfg_t.Create(16),
                    LineName = LineName.ZW
                };
                configs.Cards.Add(card);
            }
            for (int i = 1; i <= 160; i++)
            {
                string type;
                switch (i % 4)
                {
                    case 1:
                        type = "X";
                        break;
                    case 2:
                    case 0:
                    default:
                        type = "Z";
                        break;
                    case 3:
                        type = "Y";
                        break;
                }
                if (i <= 80)
                {
                    if (i > 68) continue;
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"ZWK-{(i - 1) % 5 + 1:00}-{(i - 1) / 5 + 1:00}{type}",
                        Channel = (i - 1) / 5 + 1,
                        Index = i,
                        Config = chan_cfg_t.Create((i - 1) / 5, type),
                        Ip = $"192.168.1.{49 + (i - 1) % 5 + 1:00}",
                        ProbeName = $"ZWT-{i:000}{type}",
                        CardName = $"ZWK-{(i - 1) % 5 + 1:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.ZW
                    };
                    configs.Probes.Add(probe);
                }
                else
                {
                    if (i > 148) continue;
                    ProbeInfo probe = new ProbeInfo()
                    {
                        ChannelName = $"ZWK-{(i - 1) % 5 + 6:00}-{(i - 81) / 5 + 1:00}{type}",
                        Channel = (i - 81) / 5 + 1,
                        Index = i - 12,
                        Config = chan_cfg_t.Create((i - 81) / 5, type),
                        Ip = $"192.168.1.{49 + (i - 1) % 5 + 6:00}",
                        ProbeName = $"ZWT-{i - 12:000}{type}",
                        CardName = $"ZWK-{(i - 1) % 5 + 6:00}",
                        ProbeTypeCode = type,
                        LineName = LineName.ZW
                    };
                    configs.Probes.Add(probe);
                }
            }
            foreach (var probe in configs.Probes)
            {
                switch (probe.ProbeTypeCode)
                {
                    default:
                        throw new Exception($"无效的ProbeTypeCode值{probe.ProbeTypeCode}");
                    case "Z":
                        probe.ProbeType = "直探头";
                        break;
                    case "Y":
                        probe.ProbeType = "斜探头-轮缘";
                        break;
                    case "X":
                        probe.ProbeType = "斜探头";
                        break;
                }
            }
            string info = JsonConvert.SerializeObject(configs);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameof(CardConfigurations), "CardConfigs.cfg");
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameof(CardConfigurations)));
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(info);
                sw.Flush();
            }
        }

    }
}
