using CardConfigurations;
using Eth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace 探伤算法
{
    /// <summary>
    /// 样板轮判定算法
    /// </summary>
    public class ModelWheelInspect
    {
        public List<DefectInfo> DefectInfos = new List<DefectInfo>();
        private CardConfigs Configs;
        public double AnglePerProbe = (26 / (Math.PI * double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"])) * 360);
        public ModelWheelInspect(List<InspectionResult> inspectionResults, CardConfigs configs = null)
        {
            foreach (var result in inspectionResults)
            {
                DefectInfos.AddRange(result.DefectInfos);
            }
            if(configs == null)
            {
                configs = CardConfigs.ReadFromFile();
            }
        }
        public string GetInspectionConclusion()
        {
            StringBuilder conclusion = new StringBuilder();
            (IEnumerable<DefectInfo> FlatBottomHoleLeft, IEnumerable<DefectInfo> FlatBottomHoleRight) = GetFlatBottomHoleAngle();
            //平底孔的角度
            var angleLeft = FlatBottomHoleLeft.OrderByDescending(f => f.Area).First().Angle;
            conclusion.Append($"左侧大平底的角度为{angleLeft.Round(2)},可探测到的探头有");
            foreach(var defect in FlatBottomHoleLeft)
            {
                conclusion.Append(defect.ProbeDataInfo.ProbeName).Append(",");
            }
            conclusion.Append("\n");
            //平底孔的角度
            var angleRight = FlatBottomHoleRight.OrderByDescending(f => f.Area).First().Angle;
            conclusion.Append($"右侧大平底的角度为{angleRight.Round(2)},可探测到的探头有");
            foreach (var defect in FlatBottomHoleRight)
            {
                conclusion.Append(defect.ProbeDataInfo.ProbeName).Append(",");
            }
            conclusion.Append("\n");
            //预测斜探头位置
            var flatBottomHole = ModelWheelDefect.ModelWheelDefects.First(w => w.Type == ModelWheelDefectType.平底孔);
            var angle1 = InspectionHelper.GetAngleFromDepth(flatBottomHole.DepthMin).Item1;
            var angle2 = InspectionHelper.GetAngleFromDepth(flatBottomHole.DepthMax).Item1;
            var soundPath1 = InspectionHelper.GetSoundPathFromDepth(flatBottomHole.DepthMin).Item1;
            var soundPath2 = InspectionHelper.GetSoundPathFromDepth(flatBottomHole.DepthMax).Item1;
            var rangePositive = GetDegreeRange((angleLeft + angle1) % 360, 0, angle2 - angle1);
            var rangeNegative = GetDegreeRange(angleLeft - angle2 < 0 ? (angleLeft - angle2 + 360) : (angleLeft - angle2), 0, angle2 - angle1);
            
            var probesPositive = Configs.Probes.Where(p => (p.LineName == LineName.ZN || p.LineName == LineName.ZW) && InspectionHelper.GetDirection(p) == 1 && rangePositive(p.Index * AnglePerProbe));
            var probesNegative = Configs.Probes.Where(p => (p.LineName == LineName.ZN || p.LineName == LineName.ZW) && InspectionHelper.GetDirection(p) == 1 && rangePositive(p.Index * AnglePerProbe));








            return conclusion.ToString();
        }
        /// <summary>
        /// 判断平底孔位置
        /// </summary>
        public (IEnumerable<DefectInfo> FlatBottomHoleLeft, IEnumerable<DefectInfo> FlatBottomHoleRight) GetFlatBottomHoleAngle()
        {
            //左侧的大平底角度
            var anglesLeft = DefectInfos.Where(defect => (defect.ProbeDataInfo.Line == LineName.ZN || defect.ProbeDataInfo.Line == LineName.ZW) && defect.IsFlatBottomHole());
            //右侧的大平底角度
            var anglesRight = DefectInfos.Where(defect => (defect.ProbeDataInfo.Line == LineName.YN || defect.ProbeDataInfo.Line == LineName.YW) && defect.IsFlatBottomHole());
            return (GetFlatBottomHoleProbes(anglesLeft), GetFlatBottomHoleProbes(anglesRight));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="defects"></param>
        /// <param name="angleDiff">最大允许的偏差角度</param>
        /// <returns></returns>
        private IEnumerable<DefectInfo> GetFlatBottomHoleProbes(IEnumerable<DefectInfo> defects, double angleDiff = 10)
        {
            if (defects.Count() == 0) return null;
            IEnumerable<IEnumerable<DefectInfo>> Angles = new List<IEnumerable<DefectInfo>>();
            IEnumerable<DefectInfo> maxAngles;
            
            while(true)
            {
                //选取剩下的伤中，面积最大的伤
                var defect = defects.OrderByDescending(d => d.Area).First();
                var centralAngel = defect.Angle;

                var predicate = GetDegreeRange(centralAngel, angleDiff, angleDiff);
                maxAngles = defects.Where(d => predicate(d.Angle));
                Angles.Append(maxAngles);

                if (maxAngles.Count() == defects.Count())
                {
                    break;
                }
                else
                {
                    defects = defects.Except(maxAngles);
                }
            }
            //取预报的探头数最多的伤
            return Angles.OrderByDescending(list => list.Count()).First();
        }
        private Func<double, bool> GetDegreeRange(double CentralDegree, double Left, double Right)
        {
            return new Func<double, bool>((value) => 
            {
                if(CentralDegree + Right <= 360 && CentralDegree - Left >= 0)
                {
                    return value < CentralDegree + Right && value > CentralDegree - Left;
                }
                else if((CentralDegree + Right > 360 && CentralDegree - Left > 0))
                {
                    return (value > CentralDegree - Left && value <= 360) || (value > 0 && value < CentralDegree + Right - 360);
                }
                else if(CentralDegree + Right <= 360 && CentralDegree - Left < 0)
                {
                    return (value >= 0 && value < CentralDegree + Right) || (value <= 360 && value > CentralDegree - Left + 360);
                }
                else//这种情况，范围已经超过360°了
                {
                    return true;
                }

            });
        }

    }
}
