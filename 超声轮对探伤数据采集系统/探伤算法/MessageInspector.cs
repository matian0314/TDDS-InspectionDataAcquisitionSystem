using CardConfigurations;
using Eth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 探伤算法
{
    public class MessageInspector
    {
        public string CombinePath { get; set; } = ConfigurationManager.AppSettings["CombineFilePath"];
        public List<string> DefectProbeNames { get; set; } = new List<string>();
        public Dictionary<string, InspectionResult> InspectionResults { get; set; } = new Dictionary<string, InspectionResult>();
        /// <summary>
        /// 数据存储
        /// </summary>
        public Dictionary<ProbeDataInfo, List<byte[]>> ProbeData { get; set; } = new Dictionary<ProbeDataInfo, List<byte[]>>();
        public void Inspect()
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
                            ProbeDataInfo info = ProbeDataInfo.CreateFromConfigs(pair.Key, config);
                            if (info == null) { continue; }
                            ProbeData.Add(info, pair.Value);
                            if (InspectionHelper.Suspected(info, pair.Value))//粗略判断
                            {
                                DefectProbeNames.Add(info.ToString());
                                InspectionResults.Add(info.ToString(), InspectionManager.Inspect(info, DateTime.Now.ToString("yyyyMMddHHmmss"), pair.Value));
                            }
                        }
                    }
                }
            }
        }
    }
}
