using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 探伤报文
{
    public class ProbStatus
    {
        /// <summary>
        /// 本结构体大小 (字节) 
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// 序号  从1开始到160结束 
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// *所属检测线  1-YW 2-YN 3-ZN 4-ZW 
        /// </summary>
        public int LineIndex { get; set; }
        /// <summary>
        /// 状态  0-未连接 1-已连接 2-无数据 3-断路 4-数据异常 
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 类型  0-直探头 1-斜探头 2-斜探头-轮缘 
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 增益 dB 
        /// </summary>
        public int DB { get; set; }
        /// <summary>
        /// 声程 mm 
        /// </summary>
        public int Path { get; set; }
        /// <summary>
        /// 探头零点 mm 
        /// </summary>
        public double ProbeZero { get; set; }
        /// <summary>
        /// 波形点距 mm 
        /// </summary>
        public double WavePtIntv { get; set; }
        /// <summary>
        /// 采集轴数
        /// </summary>
        public int AxleNum { get; set; }
    }
}
