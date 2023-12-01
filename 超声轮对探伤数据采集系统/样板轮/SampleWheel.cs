using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static 探伤算法.WheelHelper;

namespace 探伤算法
{
    public static partial class SampleWheel
    {
        public static double WheelDiameter;
        static SampleWheel()
        {
            if (ConfigurationManager.AppSettings["Site"] == "佳木斯")
            {
                WheelDiameter = 1050;
            }
            else if (ConfigurationManager.AppSettings["Site"] == "佳木斯" || ConfigurationManager.AppSettings["Site"] == "新塘")
            {
                WheelDiameter = 917;
            }
        }
        public static SampleWheelInfo SampleWheelDetect(List<InspectionJsonResult> defects, string passTime, double wheelDiameter = 0)
        {
            if (wheelDiameter == 0)
            {
                wheelDiameter = double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"]);
            }
            defects = defects.Where(d => d.HasDefect != "无伤").ToList();
            return GetSampleWheelInfo(defects, wheelDiameter, passTime);
        }


        public static SampleWheelInfo GetSampleWheelInfo(List<InspectionJsonResult> defects, double wheelDiameter, string passTime)
        {

            var flatBottomHole = FlatBottomHoleResult.GetResult(defects/*, side*/);
            //存在正向和反向两种可能性
            //先考虑正向
            int[] directions = { 1, -1};
            string[] sides = { "Z", "Y" };
            int bestDirection = 1;
            string bestSide = "Z";
            int maxMatch = 0;
            foreach(var direction in directions)
            {
                foreach(var side in sides)
                {
                    int matchNum = MatchNum(defects, flatBottomHole, side, direction, wheelDiameter, passTime);
                    if(matchNum >= maxMatch)
                    {
                        matchNum = maxMatch;
                        bestDirection = direction;
                        bestSide = side;
                    }
                }
            }
            return new SampleWheelInfo(flatBottomHole.Angle, bestDirection, bestSide, defects, passTime);
        }

        private static int MatchNum(List<InspectionJsonResult> defects, FlatBottomHoleResult flatBottomHoleAngle ,string side, int direction, double wheelDiameter, string passTime)
        {
            int matchNum = 0;
            defects = defects.Where(d => d.ProbeName.StartsWith(side)).ToList();
            SampleWheelInfo sampleWheel = new SampleWheelInfo(flatBottomHoleAngle.Angle, direction, side, defects, passTime);
            if(sampleWheel.DefectProbes.ContainsKey(SampleWheelDefectType.径向刻槽))
            {
                matchNum += sampleWheel.DefectProbes[SampleWheelDefectType.径向刻槽].Count();
            }
            if(sampleWheel.DefectProbes.ContainsKey(SampleWheelDefectType.横孔))
            {
                matchNum += sampleWheel.DefectProbes[SampleWheelDefectType.横孔].Count();
            }
            if(sampleWheel.DetectableProbes.ContainsKey(SampleWheelDefectType.平底孔))
            {
                matchNum += sampleWheel.DefectProbes[SampleWheelDefectType.平底孔].Count();
            }
            if(sampleWheel.DetectableProbes.ContainsKey(SampleWheelDefectType.踏面刻槽))
            {
                matchNum += sampleWheel.DefectProbes[SampleWheelDefectType.踏面刻槽].Count();
            }
            
            return matchNum;
        }

        
    }
}
