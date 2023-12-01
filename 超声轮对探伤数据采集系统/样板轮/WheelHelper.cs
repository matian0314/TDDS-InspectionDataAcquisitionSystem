using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 探伤算法
{
    public static partial class WheelHelper
    {
        public static readonly double ProbeInterval = 26;
        static WheelHelper()
        {
            string path = Application.CommonAppDataPath;
            string content = File.ReadAllText("Angles.txt");
            DetectableRange = JsonConvert.DeserializeObject<Dictionary<SampleWheelDefectType, List<double>>>(content);
        }
        public static int ProbeNameToIndex(string probeName)
        {
            //例如 YNT-001X
            return int.Parse(probeName.Substring(4, 3));
        }
        public static double ProbeNameToAngle(string probeName, double wheelDiameter = 0)
        {
            if(wheelDiameter == 0)
            {
                wheelDiameter = double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"]);
            }
            var wheelRadius = wheelDiameter/ 2;
            int index = ProbeNameToIndex(probeName);
            return RadToArc((index - 1) * ProbeInterval / wheelRadius);
        }
        public static int AngleToIndex(double angle, double wheelDiameter = 0)
        {
            if(wheelDiameter == 0)
            {
                wheelDiameter = double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"]);
            }
            var wheelRadius = wheelDiameter / 2;
            var rad = ArcToRad(angle);
            return (int)(rad * wheelRadius / ProbeInterval);
        }
        public static double ArcToRad(double angle, int keepDigits = 3)
        {
            return Math.Round(2 * Math.PI * angle / 360, keepDigits);
        }

        public static double RadToArc(double rad,int keepDigits = 3)
        {
            return Math.Round(360 * rad / (2 * Math.PI), keepDigits);
        }
        public static double AngleDistance(double angle1, double angle2)
        {
            angle1 %= 360;
            angle2 %= 360;
            var min = Math.Min(angle1, angle2);
            var max = Math.Max(angle1, angle2);
            return Math.Min(max-min, min + 360 - max);
        }
        public static double AnglePerProbe(double wheelDiameter)
        {
            return RadToArc(26 / wheelDiameter * 2, 3);
        }
        public static double AngleTo360(double angle)
        {
            if(angle > 360)
            {
                return angle % 360;
            }
            if(angle > 0)
            {
                return angle;
            }
            while(angle < 0)
            {
                angle += 360;
            }
            return angle;
        }
        public static Dictionary<SampleWheelDefectType, List<double>> DetectableRange;
        
        public static bool IsWithinRange(SampleWheelDefectType type, double defectCentralAngle, string probeName)
        {
            double defectAngle = WheelHelper.ProbeNameToAngle(probeName);
            if (type == SampleWheelDefectType.径向刻槽 && probeName.EndsWith("X")) return false;
            if(probeName.EndsWith("Z"))
            {
                if(type == SampleWheelDefectType.横孔)
                {
                    return Math.Abs(defectAngle - defectCentralAngle) < 5;
                }
                if(type == SampleWheelDefectType.平底孔)
                {
                    return Math.Abs(defectAngle - defectCentralAngle) < 10;
                }
                else
                {
                    //直探头其他类型的伤都探测不到
                    return false;
                }
            }
            int direction = InspectionHelper.GetDirection(probeName);
            //Dictionary<SampleWheelDefectType, List<double>> DetectableRange;
            //if (ConfigurationManager.AppSettings["Site"] == "佳木斯")
            //{
            //    DetectableRange = DetectableRangeFor1050mmWheel;
            //}
            //else
            //{
            //    DetectableRange = DetectableRangeFor917mmWheel;
            //}
            if (DetectableRange.ContainsKey(type))
            {
                List<double> detectalbeDistanceDiff = DetectableRange[type];
                var detectalbeAngleDiff = detectalbeDistanceDiff.Select(r => RadToArc(r / SampleWheel.WheelDiameter * 2)).ToList();
                var detectableAngle = detectalbeAngleDiff.Select(d => AngleTo360(defectCentralAngle - direction * d)).ToList();
                var detectableIndex = detectableAngle.Select(a => a / 2.8375 +1).ToList();
                double firstEchoLow = Math.Min(detectableAngle[0], detectableAngle[1]);
                double firstEchoHigh = Math.Max(detectableAngle[0], detectableAngle[1]);
                //由于可检测的区间一定小于180°所以，如果高低之间小于180，可探测区间在low-high，否则，落在0-low或high-360的区间内
                if (firstEchoHigh - firstEchoLow < 180)
                {
                    if (defectAngle > firstEchoLow && defectAngle < firstEchoHigh) return true;
                }
                else// if (firstEchoHigh - firstEchoLow > 180)
                {
                    if ((defectAngle > firstEchoHigh && defectAngle
                         <= 360) || (defectAngle >= 0 && defectAngle < firstEchoLow))
                    {
                        return true;
                    }
                }
                if(type == SampleWheelDefectType.踏面刻槽 && ConfigurationManager.AppSettings["Site"] == "新塘")
                {
                    //新塘的踏面刻槽只判一次波
                    return false;
                }
                if (detectalbeDistanceDiff.Count == 4)
                {
                    double secondEchoLow = Math.Min(detectableAngle[2], detectableAngle[3]);
                    double secondEchoHigh = Math.Max(detectableAngle[2], detectableAngle[3]);
                    if (secondEchoHigh - secondEchoLow < 180)
                    {
                        if (defectAngle > secondEchoLow && defectAngle < secondEchoHigh) return true;
                    }
                    else//(secondEchoHigh - secondEchoLow > 180)
                    {
                        if ((defectAngle > secondEchoHigh && defectAngle
                             <= 360) || (defectAngle >= 0 && defectAngle < secondEchoLow))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }
    }
}
