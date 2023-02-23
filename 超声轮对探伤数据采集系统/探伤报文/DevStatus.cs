using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 探伤报文
{
    /// <summary>
    /// 设备状态
    /// </summary>
    public class DevStatus
    {
        /// <summary>
        /// 本结构体大小
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// 设备ID,32
        /// </summary>
        public string DevID { get; set; }
        /// <summary>
        /// 过车车号
        /// </summary>
        public string TrainID { get; set; }
        /// <summary>
        /// yyyyMMddHHmmss/yyyyMMddHHmmss
        /// </summary>
        public string PassTime { get; set; }
        /// <summary>
        /// 采集卡状态 0-未连接 1-已连接
        /// </summary>
        public List<int> CardYW { get; set; } = new List<int>();
        /// <summary>
        /// 采集卡状态 0-未连接 1-已连接
        /// </summary>
        public List<int> CardYN { get; set; } = new List<int>();
        /// <summary>
        /// 采集卡状态 0-未连接 1-已连接
        /// </summary>
        public List<int> CardZN { get; set; } = new List<int>();
        /// <summary>
        /// 采集卡状态 0-未连接 1-已连接
        /// </summary>
        public List<int> CardZW { get; set; } = new List<int>();
        /// <summary>
        /// 探头状态 0-未连接 1-已连接 2-无数据 3-断路 4-数据异常
        /// </summary>
        public List<int> ProbeYW { get; set; } = new List<int>();

        /// <summary>
        /// 探头状态 0-未连接 1-已连接 2-无数据 3-断路 4-数据异常
        /// </summary>
        public List<int> ProbeYN { get; set; } = new List<int>();
        /// <summary>
        /// 探头状态 0-未连接 1-已连接 2-无数据 3-断路 4-数据异常
        /// </summary>
        public List<int> ProbeZN { get; set; } = new List<int>();
        /// <summary>
        /// 探头状态 0-未连接 1-已连接 2-无数据 3-断路 4-数据异常
        /// </summary>
        public List<int> ProbeZW { get; set; } = new List<int>();

    }
}
