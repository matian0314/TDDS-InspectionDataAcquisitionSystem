using CardConfigurations;
using Eth;
using MyLogger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using Tools;
using 探伤算法;

namespace 探伤报文
{
    public class InspectionInfo
    {
        private static readonly SubscribeLogger log = SubscribeLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static string CombinePath = ConfigurationManager.AppSettings["CombineFilePath"];
        public static string MessagePath = ConfigurationManager.AppSettings["SendFilePath"];
        public static string StoragePath = ConfigurationManager.AppSettings["StoragePath"];
        public static string MessageStoragePath = ConfigurationManager.AppSettings["InspectionMessageStoragePath"];
        public static int WaveNumPerProbe = Convert.ToInt32(ConfigurationManager.AppSettings["WaveNumPerProbe"]);
        public List<InspectionMsg> InspectionMsg { get; set; } = new List<InspectionMsg>();
        public List<DefectMsg> DefectMsg { get; set; } = new List<DefectMsg>();
        public SampleWheelInfo LeftWheel { get; set; } = null;
        public SampleWheelInfo RightWheel { get; set; } = null;
        public static InspectionInfo Create(FileInfo file)
        {
            if (file is null || !File.Exists(file.FullName))
            {
                throw new ArgumentNullException(nameof(file));
            }
            using (StreamReader sr = File.OpenText(file.FullName))
            {
                string json = sr.ReadToEnd();
                InspectionInfo result = JsonConvert.DeserializeObject<InspectionInfo>(json);
                return result;
            }
        }
        static InspectionInfo()
        {

        }

        public static void DataRepo_RepoWrittenComplete(string passTime)
        {
            log.Info("开始生成探伤判别报文");

            try
            {
                var config = GetCardConfigs();
                var axleNum = GetAxleNum();
                InspectionInfo info = new InspectionInfo();
                //写入常规探伤信息
                InspectionMsg msg = new InspectionMsg();
                msg.DevStatus = new DevStatus()
                {
                    PassTime = $"{passTime}/{passTime}"
                };
                List<ProbeDetails> details = CreateProbeDetails(config, axleNum);
                msg.YW = details.Where(d => d.Status.LineIndex == 1).ToList();
                msg.YN = details.Where(d => d.Status.LineIndex == 2).ToList();
                msg.ZN = details.Where(d => d.Status.LineIndex == 3).ToList();
                msg.ZW = details.Where(d => d.Status.LineIndex == 4).ToList();
                info.InspectionMsg.Add(msg);
                //写入异常检测信息
                DefectMsg defectMsg = new DefectMsg();
                Dictionary<ProbeDataInfo, InspectionResult> InspectionResults = new Dictionary<ProbeDataInfo, InspectionResult>();
                foreach (var file in Directory.GetFiles(CombinePath, "*.dat"))
                {
                    string json = "";
                    using (StreamReader sr = File.OpenText(file))
                    {
                        while (!sr.EndOfStream)
                        {
                            json = sr.ReadLine();
                            if (string.IsNullOrEmpty(json))
                            {
                                continue;
                            }
                            else
                            {
                                KeyValuePair<RepoKey, List<byte[]>> pair = JsonConvert.DeserializeObject<KeyValuePair<RepoKey, List<byte[]>>>(json);
                                ProbeDataInfo probeDataInfo = ProbeDataInfo.CreateFromConfigs(pair.Key, config);
                                if(probeDataInfo == null) { continue; }
                                if (InspectionHelper.Suspected(probeDataInfo, pair.Value))
                                {
                                    InspectionResults.Add(probeDataInfo, InspectionManager.Inspect(probeDataInfo, passTime, pair.Value));
                                }
                            }
                        }
                    }
                }
                var realDefects = InspectionResults.Where(r => r.Value.AlarmLevel > 1).ToList();
                foreach (var defects in realDefects)
                {
                    var defect = defects.Value.DefectInfos.Where(d => d.IsDefect()).FirstOrDefault();
                    var probeInfo = defects.Key;
                    var probeConfig = config.Probes.FirstOrDefault(p => p.ProbeName == probeInfo.ProbeName);
                    var probeStatus = details.First(d => d.Status.LineIndex == (int)probeInfo.Line && d.Status.Index == probeInfo.ProbeIndex + 1).Status;
                    if (defect != null && probeConfig != null)
                    {
                        int defectIndex = 1;
                        DefectDetail defectDetail = new DefectDetail()
                        {

                            Info = new DefectInfo()
                            {
                                AxleIndex = probeInfo.AxleIndex + 1,
                                DefIndex = defectIndex++,
                                DefPath = Math.Round(defect.SoundPath, 2),
                                DefWheelAngle = Math.Round(defect.Angle, 2),
                                WavePtIntv = probeStatus.WavePtIntv,
                                ProbAngle = Math.Round(probeConfig.Index * (26 / (Math.PI * defect.WheelDiameter) * 360), 2),
                                WaveNum = WaveNumPerProbe,
                                ProbName = probeConfig.ProbeName,
                                WheelPos = (probeConfig.LineName == LineName.YW || probeConfig.LineName == LineName.YN) ? 2 : 1,
                                ProbType = probeStatus.Type,
                                Size = 0,
                                ProbZero = Math.Round(probeStatus.ProbeZero, 2),
                                DefLevel = defects.Value.AlarmLevel,
                                DefDepth = Math.Round(defect.Depth, 2),
                                DefAmp = Math.Round(defect.Average25 / 2.55, 2)
                            },
                        };
                        defectMsg.Data.Add(defectDetail);
                    }

                }
                defectMsg.Sum = new DefectSum()
                {
                    DefectNum = defectMsg.Data.Count(),
                    PassTime = passTime
                };
                info.DefectMsg.Add(defectMsg);
                FileHelper.CreateFile(Path.Combine(MessagePath, $"{passTime}Inspection{ConfigurationManager.AppSettings["Side"]}.txt"), JsonConvert.SerializeObject(info));
                log.Info("开始生成探伤判别报文");
                FileHelper.CreateFile(Path.Combine(MessageStoragePath, $"{passTime}Inspection{ConfigurationManager.AppSettings["Side"]}.txt"), JsonConvert.SerializeObject(info));
                foreach (var file in Directory.GetFiles(CombinePath))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord());
                
            }
        }
        public static void CreateInspectionFileWithPythonScript(string passTime)
        {
            log.Info("开始进行探伤判别");
            log.Info($"调用探伤判别程序");
            string fileName = InspectionManager.CallPythonScript(passTime);
            var inspectionDefects = InspectionJsonResult.ReadJsonResultFile(fileName);
            SampleWheelInfo sampleWheelInfo = null;
            if(DataRepo.IsSampleWheel)
            {
                sampleWheelInfo = SampleWheel.SampleWheelDetect(inspectionDefects, passTime);
                if (ConfigurationManager.AppSettings["Site"] == "新塘")
                {
                    foreach(var probeName in sampleWheelInfo.DefectProbes[SampleWheelDefectType.踏面刻槽])
                    {
                        if(!inspectionDefects.Any(d => d.ProbeName == probeName))
                        {
                            inspectionDefects.Add(new InspectionJsonResult()
                            {
                                AxleNum = 1,
                                Confidence = 1,
                                HasDefect = "有伤",
                                 ProbeName = probeName,
                                 FileName = ""
                            });
                        }
                    }
                }
            }
            
            try
            {
                var config = GetCardConfigs();
                var axleNum = GetAxleNum();
                InspectionInfo info = new InspectionInfo();
                if (ConfigurationManager.AppSettings["Side"] == "左侧")
                {
                    info.LeftWheel = sampleWheelInfo;
                }
                else
                {
                    info.RightWheel = sampleWheelInfo;
                }
                //写入常规探伤信息
                InspectionMsg msg = new InspectionMsg();
                msg.DevStatus = new DevStatus()
                {
                    PassTime = $"{passTime}/{passTime}"
                };
                List<ProbeDetails> details = CreateProbeDetails(config, axleNum);
                msg.YW = details.Where(d => d.Status.LineIndex == 1).ToList();
                msg.YN = details.Where(d => d.Status.LineIndex == 2).ToList();
                msg.ZN = details.Where(d => d.Status.LineIndex == 3).ToList();
                msg.ZW = details.Where(d => d.Status.LineIndex == 4).ToList();
                info.InspectionMsg.Add(msg);
                //写入异常检测信息
                DefectMsg defectMsg = new DefectMsg();
                Dictionary<ProbeDataInfo, InspectionResult> InspectionResults = new Dictionary<ProbeDataInfo, InspectionResult>();
                foreach (var file in Directory.GetFiles(CombinePath, "*.dat"))
                {
                    string json = "";
                    using (StreamReader sr = File.OpenText(file))
                    {
                        while (!sr.EndOfStream)
                        {
                            json = sr.ReadLine();
                            if (string.IsNullOrEmpty(json))
                            {
                                continue;
                            }
                            else
                            {
                                KeyValuePair<RepoKey, List<byte[]>> pair = JsonConvert.DeserializeObject<KeyValuePair<RepoKey, List<byte[]>>>(json);
                                ProbeDataInfo probeDataInfo = ProbeDataInfo.CreateFromConfigs(pair.Key, config);
                                if (probeDataInfo == null) { continue; }
                                if (inspectionDefects.Any(d => d.ProbeName == probeDataInfo.ProbeName && d.HasDefect != "无伤" && d.AxleNum == probeDataInfo.AxleIndex + 1))
                                {
                                    var defect = inspectionDefects.First(d => d.ProbeName == probeDataInfo.ProbeName && d.HasDefect != "无伤" && d.AxleNum == probeDataInfo.AxleIndex + 1);
                                    var inspectResult = InspectionManager.Inspect(probeDataInfo, passTime, pair.Value);
                                    if(inspectResult.DefectInfos.Count > 0)
                                    {
                                        inspectResult.DefectInfos.First(d => d.EstimatedRealArea == inspectResult.DefectInfos.Max(dInfo => dInfo.EstimatedRealArea)).AlarmLevel = defect.Confidence > 0.95 ? 3 : 2;
                                    }
                                    
                                    InspectionResults.Add(probeDataInfo, inspectResult);
                                }
                            }
                        }
                    }
                }
                var realDefects = InspectionResults.Where(r => r.Value.AlarmLevel > 1).ToList();
                foreach (var defects in realDefects)
                {
                    var defect = defects.Value.DefectInfos.Where(d => d.IsDefect()).FirstOrDefault();
                    var probeInfo = defects.Key;
                    var probeConfig = config.Probes.FirstOrDefault(p => p.ProbeName == probeInfo.ProbeName);
                    var probeStatus = details.First(d => d.Status.LineIndex == (int)probeInfo.Line && d.Status.Index == probeInfo.ProbeIndex + 1).Status;
                    if (defect != null && probeConfig != null)
                    {
                        int defectIndex = 1;
                        DefectDetail defectDetail = new DefectDetail()
                        {

                            Info = new DefectInfo()
                            {
                                AxleIndex = probeInfo.AxleIndex + 1,
                                DefIndex = defectIndex++,
                                DefPath = Math.Round(defect.SoundPath, 2),
                                DefWheelAngle = Math.Round(defect.Angle, 2),
                                WavePtIntv = probeStatus.WavePtIntv,
                                ProbAngle = Math.Round(probeConfig.Index * (26 / (Math.PI * defect.WheelDiameter) * 360), 2),
                                WaveNum = WaveNumPerProbe,
                                ProbName = probeConfig.ProbeName,
                                WheelPos = (probeConfig.LineName == LineName.YW || probeConfig.LineName == LineName.YN) ? 2 : 1,
                                ProbType = probeStatus.Type,
                                Size = 0,
                                ProbZero = Math.Round(probeStatus.ProbeZero, 2),
                                DefLevel = defects.Value.AlarmLevel,
                                DefDepth = Math.Round(defect.Depth, 2),
                                DefAmp = Math.Round(defect.Average25 / 2.55, 2)
                            },
                        };
                        defectMsg.Data.Add(defectDetail);
                    }

                }
                defectMsg.Sum = new DefectSum()
                {
                    DefectNum = defectMsg.Data.Count(),
                    PassTime = passTime
                };
                info.DefectMsg.Add(defectMsg);
                FileHelper.CreateFile(Path.Combine(MessagePath, $"{passTime}Inspection{ConfigurationManager.AppSettings["Side"]}.txt"), JsonConvert.SerializeObject(info));
                log.Info($"开始生成探伤判别报文{Path.Combine(MessageStoragePath, $"{passTime}Inspection{ConfigurationManager.AppSettings["Side"]}.txt")}");
                FileHelper.CreateFile(Path.Combine(MessageStoragePath, $"{passTime}Inspection{ConfigurationManager.AppSettings["Side"]}.txt"), JsonConvert.SerializeObject(info));
                foreach (var file in Directory.GetFiles(CombinePath))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord());

            }
        }
        private static int GetAxleNum()
        {
            var files = Directory.GetFiles(CombinePath, "*.dat");
            if(files.Count() == 0)
            {
                return 0;
            }
            else
            {
                return files.Select(f => Path.GetFileNameWithoutExtension(f)).Select(f => int.Parse(f.Split('-').Last())).Max();
            }

        }

        private static CardConfigs GetCardConfigs()
        {
            
            var configFile = Directory.GetFiles(CombinePath, "Configs*.cfg");
            CardConfigs config;
            if (configFile.Count() > 0)
            {
                config = CardConfigs.ReadFromFile(configFile[0]);
            }
            else
            {
                config = CardConfigs.ReadFromFile();
            }
            return config;
        }

        public static List<ProbeDetails> CreateProbeDetails(CardConfigs configs, int axleNum)
        {
            var result = new List<ProbeDetails>();
            foreach(var config in configs.Probes)
            {
                var soundSpeed = config.ProbeType == "直探头" ? 5900 : 3230;
                int type = 0;
                switch(config.ProbeType)
                {
                    case "直探头":
                        type = 0;
                        break;
                    case "斜探头":
                        type = 1;
                        break;
                    case "斜探头-轮缘":
                        type = 2;
                        break;
                }
                ProbeDetails detail = new ProbeDetails()
                {
                    Status = new ProbStatus()
                    {
                        DB = (int)config.Db,
                        AxleNum = axleNum,
                        Index = config.Index,
                        LineIndex = (int)config.LineName,
                        Path = (int)config.SoundPath,
                        //声速(m/s) * sample_width(us) / 200000
                        ProbeZero = Math.Round(config.Config.sample_delay * soundSpeed / 200000, 2),
                        Size = 0,
                        Status = 1,
                        WavePtIntv = Math.Round(config.SoundPath / 500, 3),
                        Type = type
                    },
                    Waves = new List<AxleWave>()
                };
                result.Add(detail);
            }
            return result;
        }
    }
}
