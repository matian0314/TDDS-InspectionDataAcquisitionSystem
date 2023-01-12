using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace 探伤报文
{
    public class DefectInfo
    {
        /// <summary>
        /// 本结构体大小 (字节) 
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// 缺陷序号 (从 1 开始) 
        /// </summary>
        public int DefIndex { get; set; }

        /// <summary>
        /// 轮对位置 (车轴序号，从 1 开始) 
        /// </summary>
        public int AxleIndex { get; set; }
        /// <summary>
        /// 缺陷轮位 1-左轮 2右轮 
        /// </summary>
        public int WheelPos { get; set; }
        /// <summary>
        /// 缺陷等级 
        /// </summary>
        public int DefLevel { get; set; }
        /// <summary>
        /// 通道名称 
        /// </summary>
        public string ProbName { get; set; }
        /// <summary>
        /// 探头类型 0-直探头 1-斜探头 2-斜探头-轮缘 
        /// </summary>
        public int ProbType { get; set; }
        /// <summary>
        /// 探头角度°
        /// </summary>
        public double ProbAngle { get; set; }
        /// <summary>
        /// 探头零点 mm 
        /// </summary>
        public double ProbZero { get; set; }
        /// <summary>
        /// 缺陷声程 mm 
        /// </summary>
        public double DefPath { get; set; }
        /// <summary>
        /// 缺陷波高 % 
        /// </summary>
        public double DefAmp { get; set; }
        /// <summary>
        /// 缺陷轮圆周角度 °(第一个探头位置为 0°) 
        /// </summary>
        public double DefWheelAngle { get; set; }
        /// <summary>
        /// 缺陷径向深度 mm 
        /// </summary>

        public double DefDepth { get; set; }
        /// <summary>
        /// 波形个数 
        /// </summary>
        public int WaveNum { get; set; }
        /// <summary>
        /// 波形点距 mm 
        /// </summary>
        public double WavePtIntv { get; set; }
    }
}
