using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _485通信._485协议
{
    public class ShutDownSignal
    {
        /// <summary>
        /// 帧头
        /// </summary>
        public int Head { get; set; }
        /// <summary>
        /// 帧长
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 命令字,关机信号为0x6F
        /// </summary>
        public byte Command { get; set; }
        public byte Reserve1 { get; set; }
        /// <summary>
        /// 轴数低位
        /// </summary>
        public byte AxleCntLow { get; set; }
        /// <summary>
        /// 轴数高位
        /// </summary>
        public byte AxleCntHigh { get; set; }
        public int AxleCnt
        {
            get { return 256 * AxleCntHigh + AxleCntLow; }
        }
        /// <summary>
        /// 帧尾
        /// </summary>
        public byte Tail { get; set; }

        public bool ValidMessage { get; set; }
        public ShutDownSignal() { }
        public ShutDownSignal(byte[] message)
        {
            ValidMessage = true;

            Head = message[0];
            if (Head != 0xF5)
            {
                ValidMessage = false;
            }

            Length = message[1];
            if (Length != 0x07)
            {
                ValidMessage = false;
            }

            Command = message[2];
            if (Command != 0x6F)
            {
                ValidMessage = false;
            }

            Reserve1 = message[3];

            AxleCntHigh = message[4];
            AxleCntLow = message[5];

            Tail = message[6];
        }
    }
}
