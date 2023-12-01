using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace 探伤算法
{
    public class FlatBottomHoleResult
    {
        public int ProbeNum { get; set; }
        public List<string> ProbeNames { get; set; }
        public double Angle { get; set; }
        public static FlatBottomHoleResult GetResult(List<InspectionJsonResult> defectProbes/*, string side*/, double wheelDiameter = 0, int maxIndexDiff = 5)
        {
            if (wheelDiameter == 0)
            {
                wheelDiameter = double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"]);
            }
            double wheelRadius = wheelDiameter / 2;
            //先寻找大平底
            var straightAngles = defectProbes.Where(d => d.ProbeName.EndsWith("Z") && d.HasDefect != "无伤").ToList();
            //var straightAngles = eligible_defects.Where(d => d.ProbeName.StartsWith(side)).ToList();
            //寻找直探头，寻找[Index, Index+5]区间中，报有伤的Index，认为这个区间是大平底
            int bestIndex = 0;
            int maxPointNum = 0;
            List<int> ProbeIndexs = straightAngles.Select(d => WheelHelper.ProbeNameToIndex(d.ProbeName)).OrderBy(d => d).ToList();
            for (int i = 0; i < ProbeIndexs.Count; i++)
            {
                int currentNum = 0;
                for (int j = 0; j < ProbeIndexs.Count - i; j++)
                {
                    if (ProbeIndexs[i + j] <= ProbeIndexs[i] + maxIndexDiff)
                    {
                        currentNum++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (currentNum > maxPointNum)
                {
                    bestIndex = i;
                    maxPointNum = currentNum;
                }
            }
            double sum = 0;
            int probeNum = 0;
            for (int i = bestIndex; i < ProbeIndexs.Count; i++)
            {
                if (ProbeIndexs[i] <= ProbeIndexs[bestIndex] + maxIndexDiff)
                {
                    sum += ProbeIndexs[i];
                    probeNum++;
                }
                else
                {
                    break;
                }
            }
            double angle = WheelHelper.RadToArc(sum / probeNum * 26 / wheelRadius);
            return new FlatBottomHoleResult()
            {
                ProbeNames = straightAngles.Where(p => WheelHelper.ProbeNameToIndex(p.ProbeName) <= ProbeIndexs[bestIndex] + maxIndexDiff && WheelHelper.ProbeNameToIndex(p.ProbeName) >= ProbeIndexs[bestIndex]).Select(p => p.ProbeName).ToList(),
                ProbeNum = probeNum,
                Angle = angle,
            };
        }
    }


}
