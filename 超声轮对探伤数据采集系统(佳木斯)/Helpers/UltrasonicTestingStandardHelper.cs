using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class UltrasonicTestingStandardHelper
    {
        /// <summary>
        /// 平底孔标准 直径3mm平底孔在20mm深处，为80%/40%波高
        /// 幅值 = (100 * 波高系数)/(声程/20)^2
        /// </summary>
        private static Dictionary<string, double[]> FlatBottomHoleDict = new Dictionary<string, double[]>();
        /// <summary>
        /// 平底孔标准 直径3mm横孔在30mm深度处，为80%/40%波高
        /// 幅值 = (100 * 波高系数)/(声程比值)^(3/2)
        /// </summary>
        private static Dictionary<string, double[]> HorizontalHoleDict = new Dictionary<string, double[]>();
        /// <summary>
        /// 获取横孔的判别标准
        /// </summary>
        /// <param name="waveHeightPercentage">直径3mm横孔在30mm深度处，占百分之多少波高</param>
        /// <param name="pointInterval"></param>
        /// <param name="initialWavePulse"></param>
        /// <returns></returns>
        public static double[] GetHorizontalHoleStandard(double waveHeightPercentage, double pointInterval, double initialWavePulse = 20)
        {
            string key = $"{ waveHeightPercentage:0.00}-{pointInterval:0.000}-{initialWavePulse:0.00}";
            if (HorizontalHoleDict.ContainsKey(key))
            {
                return HorizontalHoleDict[key];
            }
            else
            {
                double[] result = new double[500];
                for (int i = 0; i < 500; i++)
                {
                    if(i * pointInterval < initialWavePulse)
                    {
                        result[i] = 100;
                    }
                    else
                    {
                        //result[i] = DACValueForHorizontalHoleSoundPathAt30mm(waveHeightPercentage, pointInterval * i);
                        result[i] = waveHeightPercentage * 100;
                    }
                }
                HorizontalHoleDict.Add(key, result);
                return result;
            }
        }

        private static double DACValueForHorizontalHoleSoundPathAt30mm(double waveHeightPercentage, double soundPath)
        {
            var soundPathOneTime = soundPath - Math.Floor(soundPath / HorizontalMaxSoundPath) * HorizontalMaxSoundPath;
            var limit = Math.Round(100 * waveHeightPercentage / Math.Pow(soundPathOneTime / HorizontalHoleSoundPathAt30mm, 1.5), 2);
            if (limit > 100)
            {
                return 100;
            }
            else if (limit < 5)//太小的波基本是干扰
            {
                return 5;
            }
            else
            {
                return limit;
            }
        }
        private static double DACValueForGetFlatBottomHoleAt20mm(double waveHeightPercentage, double soundPath)
        {
            var limit = Math.Round(100 * waveHeightPercentage / Math.Pow(soundPath / 20, 2), 2);
            if (limit > 100)
            {
                return 100;
            }
            else if (limit < 5)//太小的波基本是干扰
            {
                return 5;
            }
            else
            {
                return limit;
            }
        }

        public static double[] GetFlatBottomHole(double waveHeightPercentage, double pointInterval, double initialWavePulse = 10)
        {
            string key = $"{ waveHeightPercentage:0.00}-{pointInterval:0.000}-{initialWavePulse:0.00}";
            if (FlatBottomHoleDict.ContainsKey(key))
            {
                return FlatBottomHoleDict[key];
            }
            else
            {
                double[] result = new double[500];
                for (int i = 0; i < 500; i++)
                {
                    if (i * pointInterval < initialWavePulse)
                    {
                        result[i] = 100;
                    }
                    else
                    {
                        //result[i] = DACValueForGetFlatBottomHoleAt20mm(waveHeightPercentage, pointInterval * i);
                        result[i] = waveHeightPercentage * 100;
                    }
                }
                FlatBottomHoleDict.Add(key, result);
                return result;
            }
        }
        public static double GetPointInterval(string probeType, int TrigWidth)
        {
            double sonicSpeed;
            if(probeType == "直探头")
            {
                sonicSpeed = 5900;
            }
            else
            {
                sonicSpeed = 3230;
            }
            //采样间隔 = (TrigWidth + 1)* 10ns
            //点距 = 声速 * 采样间隔 / 2
            return Math.Round(sonicSpeed * (TrigWidth + 1) / 200_000, 3); 
        }
        /// <summary>
        /// 30mm深处的声程
        /// 三角形ABC，三边为abc，其中a为车轮半径为R,b为R-30,∠B为65°,c为声程，即HorizontalHoleSoundPath
        /// 结果有两个根，取较小的根
        /// </summary>
        private static double HorizontalHoleSoundPathAt30mm;
        /// <summary>
        /// 斜探头以65°角入射，一次波的最长声程
        /// </summary>
        private static double HorizontalMaxSoundPath;
        private static double DefaultDiameter = 920;
        static UltrasonicTestingStandardHelper()
        {
            
            //计算HorizontalHoleSoundPath
            double a = DefaultDiameter / 2;
            double cos65 = Math.Cos(Math.PI * 65 / 180);
            double b = a - 30;
            //根据余弦定理，cos65 = a^2+c^2 - b^2 / 2ac
            //得关于c的二元一次方程 c^2 - 2acos65 * c + a^2 - b^2 = 0
            //根据求根公式 c的两个根为 (2acos65 + sqrt((2acos65)^2 - 4a^2 + 4b^2))/ 2和(2acos65 - sqrt((2acos65)^2 - 4a^2 + 4b^2))/ 2
            HorizontalHoleSoundPathAt30mm = (2 * a * cos65 - Math.Sqrt(Math.Pow(2 * a * cos65, 2) - 4 * Math.Pow(a, 2) + 4 * Math.Pow(b , 2))) / 2;
            //计算最大声程 最大声程时，两边均为半径长度即R,为a边和b边，因长度相同，故两个角也均为斜入射角65°
            //根据正弦定理 c / sinC = b / sinB
            //易知 ∠C为(180 - 65 * 2) = 50
            double sin50 = Math.Sin(Math.PI * 50 / 180);
            double sin65 = Math.Sin(Math.PI * 65 / 180);
            HorizontalMaxSoundPath = DefaultDiameter / 2 * sin50 / sin65;
        }

    }
}
