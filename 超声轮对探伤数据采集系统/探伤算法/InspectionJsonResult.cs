using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace 探伤算法
{
    public class InspectionJsonResult
    {
        public string FileName { get; set; }
        public string HasDefect { get; set; }
        public double Confidence { get; set; }
        public string ProbeName { get; set; }
        public int AxleNum { get; set; }
        public static List<InspectionJsonResult> ReadJsonResultFile(string filePath)
        {
            List<InspectionJsonResult> result = new List<InspectionJsonResult>();
            if(!File.Exists(filePath))
            {
                return result;
            }
            string json = File.ReadAllText(filePath, Encoding.GetEncoding("gb2312"));
            //j[0] 斜探头 j[1] 轮缘探头 j[2] 直探头
            JArray jArray = JArray.Parse(json);
            foreach (var jObject in jArray)
            {
                JArray probes = JArray.Parse(jObject.ToString());
                foreach (var probe in probes) 
                {
                    var probeResult = new InspectionJsonResult()
                    {
                        FileName = probe[0].ToString(),
                        HasDefect = probe[1].ToString(),
                        Confidence = double.Parse(probe[2].ToString())
                    };
                    string imageFileName = Path.GetFileNameWithoutExtension(probeResult.FileName);
                    int index = imageFileName.IndexOf("Ax");
                    probeResult.AxleNum = int.Parse(imageFileName.Substring(index + 2));
                    probeResult.ProbeName = imageFileName.Substring(0, index - 1);
                    result.Add(probeResult);
                }
            }
            return result;
        }
    }
}
