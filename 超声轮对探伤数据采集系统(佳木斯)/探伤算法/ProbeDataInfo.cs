using CardConfigurations;
using Eth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 探伤报文;

namespace 探伤算法
{
    public class ProbeDataInfo
    {
        /// <summary>
        /// 属于哪一条探测线
        /// 1-YW 2-YN 3-ZN 4-ZW 
        /// </summary>
        public LineName Line { get; set; }
        /// <summary>
        /// 板卡名
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 同一条探测线上的第几块板卡
        /// 从0开始
        /// </summary>
        public int CardIndex { get; set; }
        /// <summary>
        /// 探头编号 唯一标识符
        /// </summary>
        public string ProbeName { get; set; }
        /// <summary>
        /// 探头在板卡中是第几通道
        /// 从0开始
        /// </summary>
        public int ChannelIndex { get; set; }
        /// <summary>
        /// 探头在整条探测线(YN,YW,ZN,ZW)上是第几通道
        /// 编号从0开始
        /// </summary>
        public int ProbeIndex { get; set; }
        /// <summary>
        /// 探头在整条探测线(YN,YW,ZN,ZW)的
        /// 同种探头(直探头、斜探头、轮缘探头)是第几通道
        /// 编号从0开始
        /// </summary>
        public int TypeIndex { get; set; }
        /// <summary>
        /// 直探头(0)
        /// 斜探头(1)
        /// 轮缘探头(2)
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 增益值 单位db
        /// </summary>
        public double Gain { get; set; }
        /// <summary>
        /// 波形点距 单位mm
        /// </summary>
        public double PointInterval { get; set; }
        /// <summary>
        /// 探头始波范围 单位mm
        /// 生成波形时，忽略始波
        /// </summary>
        public double ProbeZero { get; set; }
        /// <summary>
        /// 有效的声程范围
        /// 超出该范围的波形不认为是伤波
        /// 单位 mm
        /// </summary>
        public double EffectiveSoundPathLow { get; set; }
        /// <summary>
        /// 有效的声程范围
        /// 超出该范围的波形不认为是伤波
        /// 单位 mm
        /// </summary>
        public double EffectiveSoundPathHigh { get; set; }
        /// <summary>
        /// 状态  0-未连接 1-已连接 2-无数据 3-断路 4-数据异常
        /// 状态不为1(已连接)的探头，波形无效
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 探测的轴序号
        /// 从0开始
        /// </summary>
        public int AxleIndex { get; set; }
        /// <summary>
        /// 报警级别
        /// 0级 未经判断
        /// 1级 正常
        /// 2级 跟踪观察
        /// 3级 复查判断
        /// </summary>
        public int AlarmLevel { get; set; }
        /// <summary>
        /// 人工复核
        /// 0 未经人工复核
        /// 1 复核正常
        /// 2 怀疑有伤
        /// 3 确认有伤
        /// </summary>
        public int ManualRecheck { get; set; } = 0;
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
        public override string ToString()
        {
            return $"Line {Line}-Probe {ProbeName}-Axle {AxleIndex}";
        }
        public string ToBriefString()
        {
            return $"{ProbeName}-{AxleIndex + 1}";
        }
        static ProbeDataInfo()
        {
            CurrentCardConfigs = CardConfigs.ReadFromFile();
            CurrentProbes = CreateCurrentProbes(CurrentCardConfigs);
        }
        public static CardConfigs CurrentCardConfigs { get; set; }
        public static List<ProbeDataInfo> CurrentProbes { get; } = new List<ProbeDataInfo>();
        public static List<ProbeDataInfo> CreateCurrentProbes(CardConfigs Card)
        {
            List<ProbeDataInfo> result = new List<ProbeDataInfo>();
            foreach (var probe in Card.Probes)
            {
                CardInfo card = Card.Cards.First(c => probe.Ip == c.Ip);
                ProbeDataInfo info = new ProbeDataInfo()
                {
                    AlarmLevel = 0,
                    CardName = probe.CardName,
                    Line = probe.LineName,
                    AxleIndex = 0,
                    Type = probe.ProbeType,
                    ProbeName = probe.ProbeName,
                    ChannelIndex = probe.Channel - 1,
                    ProbeIndex = probe.Index - 1,
                    CardIndex = card.Index - 1,
                    Gain = probe.Db,
                };
                switch (probe.ProbeType)
                {
                    case "直探头":
                        info.EffectiveSoundPathHigh = 40;
                        info.EffectiveSoundPathLow = 10;
                        info.TypeIndex = info.ProbeIndex / 2;
                        break;
                    case "斜探头":
                        info.EffectiveSoundPathHigh = 300;
                        info.EffectiveSoundPathLow = 20;//原为30,太低，改为60
                        info.TypeIndex = info.ProbeIndex / 4;
                        break;
                    case "斜探头-轮缘":
                        info.EffectiveSoundPathHigh = 600;
                        info.EffectiveSoundPathLow = 20;
                        info.TypeIndex = info.ProbeIndex / 4;
                        break;
                }
                result.Add(info);
            }
            return result;
        }
        public ProbeDataInfo() { }
        public static ProbeDataInfo CreateFromProbeStatus(ProbStatus probeStatus, int AxleIndex, CardConfigs configs = null)
        {
            if (configs == null) configs = CurrentCardConfigs;
            ProbeInfo probeInfo = configs.Probes.FirstOrDefault(p => probeStatus.LineIndex == (int)p.LineName && probeStatus.Index == p.Index);
            if (probeInfo == null) throw new Exception($"板卡配置文件错误，未找到{probeStatus.LineIndex}号线{probeStatus.Index}探头配置信息");
            CardInfo cardInfo = configs.Cards.FirstOrDefault(c => c.Ip == probeInfo.Ip);
            ProbeDataInfo probeDataInfo = new ProbeDataInfo()
            {
                AlarmLevel = 0,
                CardName = probeInfo.CardName,
                Line = probeInfo.LineName,
                AxleIndex = AxleIndex,
                Type = probeInfo.ProbeType,
                ProbeName = probeInfo.ProbeName,
                ChannelIndex = probeInfo.Channel - 1,
                ProbeIndex = probeInfo.Index - 1,
                CardIndex = cardInfo.Index - 1,
                PointInterval = probeStatus.WavePtIntv,
                Gain = probeStatus.DB,
                ProbeZero = probeStatus.ProbeZero,
                Status = probeStatus.Status,
            };
            switch (probeInfo.ProbeType)
            {
                case "直探头":
                    probeDataInfo.EffectiveSoundPathHigh = 60;
                    probeDataInfo.EffectiveSoundPathLow = 10;
                    probeDataInfo.TypeIndex = probeDataInfo.ProbeIndex / 2;
                    break;
                case "斜探头":
                    probeDataInfo.EffectiveSoundPathHigh = 400;
                    probeDataInfo.EffectiveSoundPathLow = 30;//原为30,太低，改为60
                    probeDataInfo.TypeIndex = probeDataInfo.ProbeIndex / 4;
                    break;
                case "斜探头-轮缘":
                    probeDataInfo.EffectiveSoundPathHigh = 600;
                    probeDataInfo.EffectiveSoundPathLow = 30;
                    probeDataInfo.TypeIndex = probeDataInfo.ProbeIndex / 4;
                    break;
            }
            return probeDataInfo;
        }
        public static ProbeDataInfo CreateFromConfigs(RepoKey key, CardConfigs configs = null)
        {
            if (configs == null) configs = CurrentCardConfigs;
            ProbeInfo probeInfo = configs.Probes.FirstOrDefault(p => key.Ip == p.Ip && key.Channel == p.Channel - 1);
            if (probeInfo == null) throw new Exception($"板卡配置文件错误，未找到Ip为{key.Ip}的{key.Channel + 1}通道的探头配置信息");
            CardInfo cardInfo = configs.Cards.FirstOrDefault(c => c.Ip == probeInfo.Ip);
            ProbeDataInfo probeDataInfo = new ProbeDataInfo()
            {
                AlarmLevel = 0,
                CardName = probeInfo.CardName,
                Line = probeInfo.LineName,
                AxleIndex = key.Axle - 1,
                Type = probeInfo.ProbeType,
                ProbeName = probeInfo.ProbeName,
                ChannelIndex = probeInfo.Channel - 1,
                ProbeIndex = probeInfo.Index - 1,
                CardIndex = cardInfo.Index - 1,
                PointInterval = probeInfo.SoundPath / 500,
                Gain = probeInfo.Db,
                Status = 1,//有数据就是正常的
            };
            switch (probeInfo.ProbeType)
            {
                case "直探头":
                    probeDataInfo.EffectiveSoundPathHigh = 60;
                    probeDataInfo.EffectiveSoundPathLow = 10;
                    probeDataInfo.TypeIndex = probeDataInfo.ProbeIndex / 2;
                    probeDataInfo.ProbeZero = probeInfo.Config.sample_delay * 5900 / 200000;
                    break;
                case "斜探头":
                    probeDataInfo.EffectiveSoundPathHigh = 150;
                    probeDataInfo.EffectiveSoundPathLow = 50;//原为30,太低，改为60
                    probeDataInfo.TypeIndex = probeDataInfo.ProbeIndex / 4;
                    probeDataInfo.ProbeZero = probeInfo.Config.sample_delay * 3230 / 200000;
                    break;
                case "斜探头-轮缘":
                    probeDataInfo.EffectiveSoundPathHigh = 600;
                    probeDataInfo.EffectiveSoundPathLow = 50;
                    probeDataInfo.TypeIndex = probeDataInfo.ProbeIndex / 4;
                    probeDataInfo.ProbeZero = probeInfo.Config.sample_delay * 3230 / 200000;
                    break;
            }
            return probeDataInfo;
        }
    }
}
