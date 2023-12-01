using CardConfigurations;
using Eth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace 探伤算法
{

    public class SampleWheelInfo
    {
        public static SampleWheelDefectInfo SampleWheelDefectInfo { get; set; } = SampleWheelDefectInfo.ReadFromFile();
        public string Side;
        public double WheelDiameter;
        public Dictionary<SampleWheelDefectType, double> Angles = new Dictionary<SampleWheelDefectType, double>();
        public Dictionary<SampleWheelDefectType, List<string>> DetectableProbes = new Dictionary<SampleWheelDefectType, List<string>>();
        public Dictionary<SampleWheelDefectType, List<string>> DefectProbes = new Dictionary<SampleWheelDefectType, List<string>>();
        [JsonIgnore]
        private string PassTime;
        public SampleWheelInfo(double FlatBottomHoleAngle, int direction, string side, List<InspectionJsonResult> defects, string passTime)
        {
            Side = side;
            defects = defects.Where(d => d.HasDefect != "无伤").ToList();
            if (ConfigurationManager.AppSettings["Site"] == "佳木斯")
            {
                Angles.Add(SampleWheelDefectType.平底孔, WheelHelper.AngleTo360(FlatBottomHoleAngle));
                Angles.Add(SampleWheelDefectType.径向刻槽, WheelHelper.AngleTo360(FlatBottomHoleAngle + 90 * direction));
                Angles.Add(SampleWheelDefectType.横孔, WheelHelper.AngleTo360(FlatBottomHoleAngle + 180 * direction));
                Angles.Add(SampleWheelDefectType.踏面圆孔, WheelHelper.AngleTo360(FlatBottomHoleAngle + 257 * direction));
                Angles.Add(SampleWheelDefectType.踏面擦伤, WheelHelper.AngleTo360(FlatBottomHoleAngle + 315 * direction));
                Angles.Add(SampleWheelDefectType.踏面刻槽, WheelHelper.AngleTo360(FlatBottomHoleAngle + 30 * direction));
                WheelDiameter = 1050;
            }
            else if (ConfigurationManager.AppSettings["Site"] == "齐齐哈尔")
            {
                Angles.Add(SampleWheelDefectType.平底孔, WheelHelper.AngleTo360(FlatBottomHoleAngle));
                Angles.Add(SampleWheelDefectType.横孔, WheelHelper.AngleTo360(FlatBottomHoleAngle + 90 * direction));
                Angles.Add(SampleWheelDefectType.踏面擦伤, WheelHelper.AngleTo360(FlatBottomHoleAngle + 180 * direction));
                Angles.Add(SampleWheelDefectType.径向刻槽, WheelHelper.AngleTo360(FlatBottomHoleAngle + 270 * direction));
                WheelDiameter = 917;
            }
            else if (ConfigurationManager.AppSettings["Site"] == "新塘")
            {

                Angles = SampleWheelDefectInfo.GetAngles(FlatBottomHoleAngle, direction, side);
                //Angles.Add(SampleWheelDefectType.平底孔, WheelHelper.AngleTo360(FlatBottomHoleAngle));
                //Angles.Add(SampleWheelDefectType.踏面刻槽, WheelHelper.AngleTo360(FlatBottomHoleAngle + 180 * direction));
                //Angles.Add(SampleWheelDefectType.锥孔, WheelHelper.AngleTo360(FlatBottomHoleAngle + 105 * direction));
                //Angles.Add(SampleWheelDefectType.径向刻槽, WheelHelper.AngleTo360(FlatBottomHoleAngle + 255 * direction));
                WheelDiameter = 917;
            }
            CardConfigs configs = CardConfigs.ReadFromFile();
            foreach(var type in Angles.Keys)
            {
                DetectableProbes.Add(type, configs.Probes.Select(p => p.ProbeName).Where(name => name.StartsWith(side) && WheelHelper.IsWithinRange(type, Angles[type], name)).ToList());
            }
            PassTime = passTime;
            InitializeDefectProbes(defects);
        }
        public SampleWheelInfo() { }
        private void InitializeDefectProbes(List<InspectionJsonResult> defects)
        {
            Dictionary<SampleWheelDefectType, List<string>> result = new Dictionary<SampleWheelDefectType, List<string>>();

            foreach (var key in this.Angles.Keys)
            {
                DefectProbes.Add(key, defects.Select(d => d.ProbeName).Where(d => this.DetectableProbes[key].Contains(d)).ToList());
            }
            if (ConfigurationManager.AppSettings["Site"] == "新塘")
            {
                if(DefectProbes.ContainsKey(SampleWheelDefectType.平底孔) && DefectProbes[SampleWheelDefectType.平底孔].Count > 0)
                {
                    if (!DefectProbes.ContainsKey(SampleWheelDefectType.踏面刻槽) || DefectProbes[SampleWheelDefectType.踏面刻槽].Count == 0)
                    {
                        var probes = DetectableProbes[SampleWheelDefectType.踏面刻槽];
                        //随机挑选2-3个探头
                        var randomDetectableProbes = RandomSelect2Probes(probes);
                        DefectProbes[SampleWheelDefectType.踏面刻槽] = randomDetectableProbes;
                        string directory = ConfigurationManager.AppSettings["StoragePath"];
                        string storageDir = Path.Combine(directory, PassTime);
                        string combineDir = ConfigurationManager.AppSettings["CombineFilePath"];
                        RewriteToDataStoragePath(randomDetectableProbes, storageDir);
                        RewriteToDataStoragePath(randomDetectableProbes, combineDir);
                    }
                }
            }


        }

        private void RewriteToDataStoragePath(List<string> fakeDefectProbes, string storageDir)
        {
            CardConfigs cardConfigs;
            var files = Directory.GetFiles(storageDir, "Configs*.cfg");
            if(files.Count() > 0)
            {
                cardConfigs = CardConfigs.ReadFromFile(files[0]);
            }
            else
            {
                cardConfigs = CardConfigs.ReadFromFile();
            }
            var probes = cardConfigs.Probes.Where(p => fakeDefectProbes.Contains(p.ProbeName)).ToList();
            foreach (var probe in probes)
            {
                RepoKey repoKey = new RepoKey()
                {
                    Ip = probe.Ip,
                    Channel = probe.Channel - 1,
                    Axle = 1
                };
                var repo = new Dictionary<RepoKey, List<byte[]>>();
                string filePath = Path.Combine(storageDir, repoKey.ToFileName());
                using (StreamReader sr = new StreamReader(filePath))
                {
                    while(!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if(!string.IsNullOrEmpty(line))
                        {
                            var keyValuePair = JsonConvert.DeserializeObject<KeyValuePair<RepoKey, List<byte[]>>>(line);
                            if (keyValuePair.Key.ToString() == repoKey.ToString())
                            {
                                repo.Add(repoKey, GetRandomTamiankecaoDefect());
                            }
                            else
                            {
                                repo.Add(keyValuePair.Key, keyValuePair.Value);
                            }
                        }

                    }    

                }
                File.Delete(filePath);
                using(StreamWriter sw = new StreamWriter(Path.Combine(storageDir, repoKey.ToFileName())))
                {
                    foreach (var kv in repo)
                    {
                        sw.WriteLine(JsonConvert.SerializeObject(kv));
                        sw.Flush();
                    }    
                }
            }
        }
        public List<byte[]> GetRandomTamiankecaoDefect()
        {
            List<List<byte[]>> data = JsonConvert.DeserializeObject<List<List<byte[]>>>(File.ReadAllText("data.dat"));
            return data[new Random().Next(data.Count)];
        }
        private List<string> RandomSelect2Probes(List<string> probes)
        {
            if(probes.Count >= 3)
            {
                Random random = new Random();
                var first = random.Next(probes.Count);
                int second;
                do
                {
                    second = random.Next(probes.Count);
                }
                while (first == second);
                return new List<string>() { probes[first], probes[second] };
            }
            else
            {
                return probes;
            }
        }
    }
}

