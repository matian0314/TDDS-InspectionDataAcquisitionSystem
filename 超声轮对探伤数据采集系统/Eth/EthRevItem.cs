using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eth
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EthRevItem
    {
        // 待处理数据
        public byte head;     // 固定0xff
        public byte chan;     // 通道
        public byte len;      // len * 4 = 字节总长度(包含head,chan等)
        public byte sign;

        public byte g1_result;
        public byte g2_result;
        public byte resv1;
        public byte g4_result;
        [MarshalAs(UnmanagedType.ByValArray)]
        public fixed byte wave[256];  // 波形(当len=2时，该item无波形信息；假设有500字节波形字节时，len = (8 + 500)/4 = 127 )
    }


}
