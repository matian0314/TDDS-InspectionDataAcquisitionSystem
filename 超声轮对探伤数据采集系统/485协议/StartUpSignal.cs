using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _485通信._485协议
{
    public class StartUpSignal
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
        /// 命令字,开机信号为0x5F
        /// </summary>
        public byte Command { get; set; }
        public byte Reserve1 { get; set; }
        /// <summary>
        /// 速度低位
        /// </summary>
        public byte SpeedLow { get; set; }
        /// <summary>
        /// 速度高位
        /// </summary>
        public byte SpeedHigh { get; set; }
        /// <summary>
        /// 帧尾
        /// </summary>
        public byte Tail { get; set; }
        /// <summary>
        /// 速度 mm/s
        /// </summary>
        public int Speed
        {
            get { return SpeedHigh * 256 + SpeedLow; }
        }
        public bool ValidMessage { get; set; }
        public StartUpSignal() { }
        public StartUpSignal(byte[] message)
        {
            ValidMessage = true;

            Head = message[0];
            if(Head != 0xF5)
            {
                ValidMessage = false;
            }

            Length = message[1];
            if(Length != 0x07)
            {
                ValidMessage = false;
            }

            Command = message[2];
            if(Command != 0x5F)
            {
                ValidMessage = false;
            }

            Reserve1 = message[3];

            SpeedHigh = message[4];
            SpeedLow = message[5];

            Tail = message[6];
        }
    }
}
