using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using Newtonsoft.Json;
using OpenCvSharp;
using Tools;

namespace 探伤算法
{
    public class DefectInfo
    {
        public DefectInfo(ProbeDataInfo probeDataInfo)
        {
            ProbeDataInfo = probeDataInfo;
        }
        [JsonProperty]
        /// <summary>
        /// 面积
        /// </summary>
        public double EstimatedRealArea
        {
            //以列车速度为9km/h计算(2.5mm/ms)
            get
            {
                return Math.Round(ProbeDataInfo.PointInterval * Speed * Area / 3.6, 2);
            }
        }
        [JsonProperty]
        /// <summary>
        /// 以点数来判断伤的大小
        /// </summary>
        public double Area { get; set; }
        [JsonProperty]
        public double WheelDiameter { get; } = double.Parse(ConfigurationManager.AppSettings["DefaultWheelDiameter"]);
        [JsonIgnore]
        /// <summary>
        /// 列车行进速度
        /// </summary>
        private double Speed { get; } = 9;
        [JsonProperty]
        public ProbeDataInfo ProbeDataInfo { get; }
        [JsonProperty]
        public double CentralX { get; set; }
        [JsonProperty]
        public double CentralY { get; set; }
        [JsonProperty]
        /// <summary>
        /// 声程 mm
        /// </summary>
        public double SoundPath
        {
            get
            {
                return Math.Round(ProbeDataInfo.PointInterval * CentralX, 2);
            }
        }
        [JsonProperty]
        /// <summary>
        /// 最左侧位置
        /// </summary>
        public int XMin { get; set; }
        [JsonProperty]
        /// <summary>
        /// 最右侧位置
        /// </summary>
        public int XMax
        {
            get => XMin + Width - 1;
        }
        [JsonProperty]
        /// <summary>
        /// 最靠上位置
        /// </summary>
        public int YMin { get; set; }
        [JsonProperty]
        /// <summary>
        /// 最靠下位置
        /// </summary>
        public int YMax
        {
            get => YMin + Height -1;
        }
        [JsonProperty]
        /// <summary>
        /// 周长
        /// </summary>
        public double Perimeter { get; set; }
        [JsonProperty]
        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; set; }
        [JsonProperty]
        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; set; }
        [JsonProperty]
        /// <summary>
        /// 人工判断的结果
        /// </summary>
        public ManualRecheckResult? ManualRecheck { get; set; }
        [JsonProperty]
        /// <summary>
        /// 计算得出的缺陷的角度(以车轮刚开始与第一个探头接触时的角度为0°)
        /// 对于直探头，计算较准确
        /// 对于斜探头，仅供参考
        /// </summary>
        public double Angle
        {
            get
            {
                //探头中心距为26mm
                //取车轮直径为1050mm，周长为3298.6mm
                //每一个探头之间的角度差距为26 / 3298.6 * 360 = 2.838 (单位°)
                var baseAngle = (ProbeDataInfo.ProbeIndex * (26 / (Math.PI * WheelDiameter) * 360));
                if (ProbeDataInfo.Type != "直探头")
                {
                    var direction = InspectionHelper.GetDirection(ProbeDataInfo);
                    var angle = InspectionHelper.GetAngleFromSoundPath(SoundPath, WheelDiameter, 65);
                    baseAngle = baseAngle + direction * angle;
                }
                while (baseAngle > 360)
                {
                    baseAngle -= 360;
                }
                return baseAngle;
            }
        }
        [JsonProperty]
        /// <summary>
        /// 计算得出的缺陷的深度
        /// 对于直探头，计算较准确
        /// 对于斜探头，仅供参考
        /// </summary>
        public double Depth
        {
            get
            {
                if(ProbeDataInfo.Type == "直探头")
                {
                    return SoundPath;
                }
                else
                {
                    return InspectionHelper.GetDepthFromSoundPath(SoundPath, WheelDiameter, 65);
                }
            }
        }
        /// <summary>
        /// 中心25点的亮度
        /// </summary>
        public double Average25 { get; set; }
        /// <summary>
        /// 轮廓内的中心亮度
        /// </summary>
        public double Average { get; set; }
        public double StandardDiavation { get; set; }
        /// <summary>
        /// 轮廓内的点在此处X方向上梯度的平均值
        /// </summary>
        public int[] GratitudeX { get; set; }
        /// <summary>
        /// 轮廓内的点在此处X方向上梯度的最大值
        /// </summary>
        public int[] MaxGratitudeX { get; set; }
        /// <summary>
        /// 轮廓内的点在此处Y方向上梯度的平均值
        /// </summary>
        public int[] GratitudeY { get; set; }
        /// <summary>
        /// 轮廓内的点在此处Y方向上梯度的最大值
        /// </summary>
        public int[] MaxGratitudeY { get; set; }
        public int[] GratitudeXCount { get; set; }
        public int[] GratitudeYCount { get; set; }

        public string GetRemark()
        {
            return $"缺陷声程{this.SoundPath.Round()},深度{this.Depth.Round()},角度{this.Angle.Round()}\n面积{this.Area.Round()},坐标X1 {this.XMin} X2 {this.XMax} Y1 {this.YMin} Y2 {this.YMax}\n宽度为{this.Width}高度为{this.Height} 25点亮度为{this.Average25.Round()},平均亮度为{this.Average.Round()}\n{ShowGratitude()}";
        }
        public bool IsFlatBottomHole()
        {
            if (ProbeDataInfo.Type != "直探头") return false;
            //声程校验
            if(SoundPath > 33 || SoundPath < 22)
            {
                return false;
            }
            //宽度的校验。经测试，宽度的分布基本符合正态分布，但略微向右倾斜
            if (Width > 60 || Width < 8)
            {
                return false;
            }
            if (Height < 9)
            {
                return false;//太小了
            }
            if(Area < 130 ||Area > 1600)
            {
                return false;
            }
            if(Width / Height > 2.5 || Width / Height < 0.6)//没有合理的宽度和长度比值
            {
                return false;
            }
            return true;
        }
        public bool IsDefect()
        {
            if (Average < 255 * 0.6 || Average25 < 255 * 0.8)
            {
                return false;
            }
            if (ProbeDataInfo.Type == "直探头")
            {
                //宽度的校验。经测试，宽度的分布基本符合正态分布，但略微向右倾斜
                if (Width > 60 || Width < 8)
                {
                    return false;
                }
                if (Height < 8)
                {
                    return false;//太小了
                }
                if (Area < 100 || Area > 1500)
                {
                    return false;
                }
                if (Width / Height > 2.5 || Width / Height < 0.6)//没有合理的宽度和长度比值
                {
                    return false;
                }
                return true;
            }
            else
            {//斜探头先做一个简单版的
                if (Width < 5 || Width > 15)
                {
                    return false;
                }
                if (Height < 8)
                {
                    return false;
                }
                if (Height / Width < 2)
                {
                    return false;
                }
                return true;
            }
        }
        private string ShowGratitude()
        {
            StringBuilder grad = new StringBuilder("");
            grad.Append("梯度X平均值:");
            foreach (var x in GratitudeX)
            {
                grad.Append(x);
                grad.Append(",");
            }
            grad.Append("\n梯度X点数:");
            foreach (var x in GratitudeXCount)
            {
                grad.Append(x);
                grad.Append(",");
            }
            grad.Append("\n梯度X最大值:");
            foreach(var x in MaxGratitudeX)
            {
                grad.Append(x);
                grad.Append(",");
            }
            grad.Append("\n梯度Y平均值:");
            foreach(var y in GratitudeY)
            {
                grad.Append(y);
                grad.Append(",");
            }
            grad.Append("\n梯度Y点数:");
            foreach (var y in GratitudeYCount)
            {
                grad.Append(y);
                grad.Append(",");
            }
            grad.Append("\n梯度Y最大值:");
            foreach (var y in MaxGratitudeY)
            {
                grad.Append(y);
                grad.Append(",");
            }
            return grad.ToString();
        }
        public int AlarmLevel { get; set; }
        //之前用来判断伤等级的方法
        //public int GetAlarmLevel()
        //{
        //    if (ManualRecheck == null)
        //    {
        //        if(IsDefect())
        //        {
        //            if (Area > 300)
        //            {
        //                return 3;
        //            }
        //            else
        //            {
        //                return 2;
        //            }
        //        }
        //        else
        //        {
        //            return 1;
        //        }
        //    }
        //    else
        //    {
        //        if(ManualRecheck == ManualRecheckResult.HasDefect)
        //        {
        //            return 3;
        //        }
        //        else
        //        {
        //            return 1;
        //        }
        //    }
        //}
    }
}
