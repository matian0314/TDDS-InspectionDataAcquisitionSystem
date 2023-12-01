using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace 探伤算法
{
    /// <summary>
    /// 记录样板轮规格
    /// </summary>
    public class SampleWheelDefectInfo
    {
        public Dictionary<SampleWheelDefectType, double> LeftAngles = new Dictionary<SampleWheelDefectType, double>();
        public Dictionary<SampleWheelDefectType, double> RightAngles = new Dictionary<SampleWheelDefectType, double>();
        public static SampleWheelDefectInfo ReadFromFile()
        {
            string fileName = "defects.cfg";
            fileName = Path.GetFullPath(fileName);
            if(!File.Exists(fileName)) 
            {
                CreateDefaultSampleWheelDefectInfo();
            };
            string content = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<SampleWheelDefectInfo>(content);
        }
        public static void CreateDefaultSampleWheelDefectInfo()
        {
            SampleWheelDefectInfo sampleWheelDefectInfo = new SampleWheelDefectInfo();
            sampleWheelDefectInfo.LeftAngles.Add(SampleWheelDefectType.平底孔, 0);
            sampleWheelDefectInfo.LeftAngles.Add(SampleWheelDefectType.踏面刻槽, 180);
            sampleWheelDefectInfo.LeftAngles.Add(SampleWheelDefectType.径向刻槽, 255);
            sampleWheelDefectInfo.LeftAngles.Add(SampleWheelDefectType.锥孔, 105);
            sampleWheelDefectInfo.RightAngles.Add(SampleWheelDefectType.平底孔, 0);
            sampleWheelDefectInfo.RightAngles.Add(SampleWheelDefectType.踏面刻槽, 180);
            sampleWheelDefectInfo.RightAngles.Add(SampleWheelDefectType.径向刻槽, 255);
            sampleWheelDefectInfo.RightAngles.Add(SampleWheelDefectType.锥孔, 105);
            using(StreamWriter sw = new StreamWriter("defects.cfg"))
            {
                sw.WriteLine(JsonConvert.SerializeObject(sampleWheelDefectInfo));
                sw.Flush();
            }
        }
        public Dictionary<SampleWheelDefectType, double> GetAngles(double flatBottomHoleAngle, int direction, string side)
        {
            var Angles = new Dictionary<SampleWheelDefectType, double>();
            Dictionary<SampleWheelDefectType, double> baseAngles;
            if(side == "Z")
            {
                baseAngles = LeftAngles;
            }
            else
            {
                baseAngles = RightAngles;
            }
            foreach(var defectType in baseAngles.Keys)
            {
                Angles.Add(
                    defectType, WheelHelper.AngleTo360(direction * baseAngles[defectType] + flatBottomHoleAngle)
                    );
            }
            return Angles;
        }
    }
}
