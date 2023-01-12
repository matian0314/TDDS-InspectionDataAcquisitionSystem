using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 探伤算法
{
    public enum ManualRecheckResult
    {
        /// <summary>
        /// 有伤
        /// </summary>
        HasDefect,
        /// <summary>
        /// 设备问题
        /// </summary>
        DeviceError,
        /// <summary>
        /// 无伤
        /// </summary>
        None
    }
}
