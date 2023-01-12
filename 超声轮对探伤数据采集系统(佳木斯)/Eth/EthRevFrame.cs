using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eth
{
    public unsafe struct EthRevFrame
    {           // 用户自定义回掉函数需处理的数据处理格式
                //C++中有typedef char eth_ip_t[4];
                // 当ip=192.168.1.10， ip[0]=192,ip[1]=168,ip[2]=1,ip[3]=10

        public fixed sbyte ip[4];
        public int len;// data区长度
                       //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte* data;                 // data指向 EthRevItem数组，按照EthRevItem格式进行解析
    }
}
