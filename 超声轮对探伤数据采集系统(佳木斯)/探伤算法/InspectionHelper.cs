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
    public static class InspectionHelper
    {
        //一组探测数据中至少有几个超限点
        public static int MinPointCount = 3;
        //所有探测数据中最多有几个超限点（过多的话肯定不是真实信号）
        public static int MaxPointCount = 3000;
        //总共至少有多少个超限的点
        public static int MinPointSum = 20;
        //至少在几组数据中发现超限
        public static int MinSetCount = 3;
        public static bool Suspected(this ProbeDataInfo config, List<byte[]> data)
        {
            var overrunSum = GetOverrunCount(config, data);
            return overrunSum > MinPointSum;
        }
        public static int GetOverrunCount(this ProbeDataInfo config, List<byte[]> data, int Threshold = 102)
        {
            var overrunSum = 0;
            for (int k = 0; k < data.Count; k++)
            {
                var value = data[k];
                var OverrunCount = 0;
                for (int i = 0; i < value.Length; i++)
                {
                    //去除无效的声程范围
                    //如始波等
                    if (config.PointInterval * i < config.EffectiveSoundPathLow || config.PointInterval * i > config.EffectiveSoundPathHigh)
                    {
                        continue;
                    }
                    //如果有大于阈值的，则进行统计处理，否则跳过
                    if (value[i] > Threshold)
                    {
                        OverrunCount++;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (OverrunCount >= MinPointCount)
                {
                    overrunSum += OverrunCount;
                }

            }
            if(overrunSum > MaxPointCount)
            {
                return 0;//说明是假的
            }
            return overrunSum;
        }
        /// <summary>
        /// 根据声程，以及车轮和角度等信息
        /// 获取该声程下，伤的深度
        /// </summary>
        /// <param name="soundPath">声程，mm</param>
        /// <param name="diameter">车轮直径，mm</param>
        /// <param name="angle">探头入射角度，°</param>
        /// <returns>深度，mm</returns>
        public static double GetDepthFromSoundPath(double soundPath, double diameter = default, double angle = default)
        {
            if (diameter == default) diameter = double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"]);
            if (angle == default) angle = 65;
            return 0.5 * diameter - Math.Sqrt(0.25 * diameter * diameter + soundPath * soundPath - MathHelper.Cos(angle) * diameter * soundPath);
        }
        /// <summary>
        /// 根据声程，以及车轮和角度等信息
        /// 获取该声程下，伤和当前角度相差的角度
        /// </summary>
        /// <param name="soundPath">声程，mm</param>
        /// <param name="diameter">车轮直径，mm</param>
        /// <param name="angle">探头入射角度，°</param>
        /// <returns>角度，°</returns>
        public static double GetAngleFromSoundPath(double soundPath, double diameter = default, double angle = default)
        {
            if (diameter == default) diameter = double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"]);
            if (angle == default) angle = 65;

            double depth = GetDepthFromSoundPath(soundPath, diameter, angle);
            return MathHelper.Asin(soundPath * MathHelper.Sin(angle) / (0.5 * diameter - depth));
        }
        /// <summary>
        /// 根据声程，以及车轮和角度等信息
        /// 获取该声程下，整个声程过程中最大的深度
        /// 当该深度超过轮辋厚度时，反射将不能存在，即不可能为一次波
        /// </summary>
        /// <param name="soundPath">声程，mm</param>
        /// <param name="diameter">车轮直径，mm</param>
        /// <param name="angle">探头入射角度，°</param>
        /// <returns>深度，mm</returns>
        public static double GetMaxDepth(double soundPath, double diameter = default, double angle = default)
        {
            if (diameter == default) diameter = double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"]);
            if (angle == default) angle = 65;

            if (soundPath > 0.5 * diameter * MathHelper.Cos(angle))
            {
                return 0.5 * (1 - MathHelper.Sin(angle)) * diameter;
            }
            else
            {
                return GetDepthFromSoundPath(soundPath, diameter, angle);
            }
        }
        /// <summary>
        /// 根据车轮直径和角度，确定一次波可能出现的最大声程
        /// </summary>
        /// <param name="diameter">车轮直径，mm</param>
        /// <param name="angle">探头入射角度，°</param>
        /// <returns>声程，mm,其中Item1小，Item2大</returns>
        public static double GetMaxSoundPath(double diameter = default, double angle = default)
        {
            if (diameter == default) diameter = double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"]);
            if (angle == default) angle = 65;

            return MathHelper.Sin(angle) * diameter * 0.5;
        }
        /// <summary>
        /// 根据伤的深度，以及车轮直径和角度，获取斜探头的声程
        /// </summary>
        /// <param name="depth">深度，mm</param>
        /// <param name="diameter">车轮直径，mm</param>
        /// <param name="angle">角度，°</param>
        /// <returns>声程，mm,其中Item1小，Item2大</returns>
        public static (double, double) GetSoundPathFromDepth(double depth, double diameter = default, double angle = default)
        {
            if (diameter == default) diameter = double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"]);
            if (angle == default) angle = 65;

            var result = MathHelper.SolveQuadraticEquation(1, -1 * MathHelper.Cos(angle) * diameter, (diameter - depth) * depth);
            if(result.Item1 < result.Item2)
            {
                return result;
            }
            else
            {
                return (result.Item2, result.Item1);
            }
        }
        /// <summary>
        /// 根据伤的深度，以及车轮直径和角度，获取能探测到伤的斜探头和伤的位置之间的角度大小
        /// </summary>
        /// <param name="depth">深度，mm</param>
        /// <param name="diameter">车轮直径，mm</param>
        /// <param name="angle">角度，°</param>
        /// <returns>角度，°</returns>
        public static (double, double) GetAngleFromDepth(double depth, double diameter = default, double angle = default)
        {
            if (diameter == default) diameter = double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"]);
            if (angle == default) angle = 65;

            var soundPaths = GetSoundPathFromDepth(depth, diameter, angle);
            double angle1 = MathHelper.Asin(soundPaths.Item1 * MathHelper.Sin(angle) / (0.5 * diameter - depth));
            double angle2 = MathHelper.Asin(soundPaths.Item2 * MathHelper.Sin(angle) / (0.5 * diameter - depth));
            return (angle1, angle2);
        }
        /*
            探头探测方向
            右侧：
            内侧：斜探头X探测方向与车轮运行方向一致，轮缘斜探头Y探测方向与车轮运行方向相反
            外侧：斜探头X探测方向与车轮运行方向相反，轮缘斜探头Y探测方向与车轮运行方向一致

            左侧：
            内侧：斜探头X探测方向与车轮运行方向一致，轮缘斜探头Y探测方向与车轮运行方向相反
            外侧：斜探头X探测方向与车轮运行方向相反，轮缘斜探头Y探测方向与车轮运行方向一致
        */
        /// <summary>
        /// 返回1，代表探测方向与车轮方向一致
        /// 返回-1，代表探测方向与车轮方向相反
        /// 返回0，代表是直探头，与车轮方向垂直
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static int GetDirection(ProbeDataInfo info)
        {
            if (info.Type == "直探头") return 0;
            if (info.Line == LineName.YW || info.Line == LineName.ZW)
            {
                if (info.Type == "斜探头")
                {
                    return -1;
                }
                else//轮缘
                {
                    return 1;
                }
            }
            else
            {
                if (info.Type == "斜探头")
                {
                    return 1;
                }
                else//轮缘
                {
                    return -1;
                }
            }
        }
        public static int GetDirection(ProbeInfo info)
        {
            if (info.ProbeType == "直探头") return 0;
            if (info.LineName == LineName.YW || info.LineName == LineName.ZW)
            {
                if (info.ProbeType == "斜探头")
                {
                    return -1;
                }
                else//轮缘
                {
                    return 1;
                }
            }
            else
            {
                if (info.ProbeType == "斜探头")
                {
                    return 1;
                }
                else//轮缘
                {
                    return -1;
                }
            }
        }
    }
}
