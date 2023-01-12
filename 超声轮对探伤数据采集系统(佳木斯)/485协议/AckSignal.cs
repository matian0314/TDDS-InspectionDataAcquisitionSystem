using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _485通信._485协议
{
    public class AckSignal
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
        /// 命令字,应答信号为0xF0
        /// </summary>
        public byte Command { get; set; }
        public byte Reserve1 { get; set; }
        public byte Reserve2 { get; set; }
        public byte Reserve3 { get; set; }
        /// <summary>
        /// 帧尾
        /// </summary>
        public byte Tail { get; set; }

        public bool ValidMessage { get; set; }
        public AckSignal(byte[] message)
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
            if (Command != 0xF0)
            {
                ValidMessage = false;
            }

            Reserve1 = message[3];
            Reserve2 = message[4];
            Reserve3 = message[5];

            Tail = message[6];
        }
    }
}
