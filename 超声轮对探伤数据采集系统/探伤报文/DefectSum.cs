using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 探伤报文
{
    /// <summary>
    /// 上传报文-缺陷报文-摘要 
    /// </summary>
    public class DefectSum
    {
        /// <summary>
        /// 本结构体大小 (字节) 
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DevID { get; set; }
        /// <summary>
        /// 过车车组号
        /// </summary>
        public string TrainID { get; set; }
        /// <summary>
        /// 过车时间
        /// </summary>

        public string PassTime { get; set; }
        /// <summary>
        /// 缺陷个数
        /// </summary>
        public int DefectNum { get; set; }


    }
}
