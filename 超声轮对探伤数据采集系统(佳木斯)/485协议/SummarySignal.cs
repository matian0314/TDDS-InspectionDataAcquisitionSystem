using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _485通信._485协议
{
    public class SummarySignal
    {
        /// <summary>
        /// 帧头
        /// </summary>
        public int Head { get; set; }
        /// <summary>
        /// 帧长
        /// </summary>
        public int Length
        {
            get { return (LengthHigh * 256) + LengthLow; }
        }
        public byte LengthHigh { get; set; }
        public byte LengthLow { get; set; }
        /// <summary>
        /// 命令字,关机信号为0xAF
        /// </summary>
        public byte Command { get; set; }
        /// <summary>
        /// 轴数低位
        /// </summary>
        public byte AxleCntLow { get; set; }
        /// <summary>
        ///轴数高位
        /// </summary>
        public byte AxleCntHigh { get; set; }
        public int AxleCnt
        {
            get { return 256 * AxleCntHigh + AxleCntLow; }
        }
        /// <summary>
        /// 轴速度 单位 0.01km/h
        /// </summary>
        public int[] Speed { get; set; }
        /// <summary>
        /// 轴距 单位为0.1m
        /// </summary>
        public int[] AxleDistance { get; set; }
        /// <summary>
        /// 帧尾
        /// </summary>
        public byte[] Tail { get; set; }

        public bool ValidMessage { get; set; }
        public SummarySignal(byte[] message)
        {
            ValidMessage = true;

            Head = message[0];
            if (Head != 0xF5)
            {
                ValidMessage = false;
            }

            LengthHigh = message[1];
            LengthLow = message[2];

            Command = message[3];
            if (Command != 0xAF)
            {
                ValidMessage = false;
            }

            AxleCntHigh = message[4];
            AxleCntLow = message[5];

            Speed = new int[256 * AxleCntHigh + AxleCntLow];
            AxleDistance = new int[256 * AxleCntHigh + AxleCntLow];

            for (int i = 0; i < 256 * AxleCntHigh + AxleCntLow; i++)
            {
                AxleDistance[i] = message[6 + 3 * i];
                Speed[i] = message[7 + 3 * i] * 256 + message[8 + 3 * i];
            }

            Tail[0] = message[6 + 3 * (256 * AxleCntHigh + AxleCntLow)];
            Tail[1] = message[7 + 3 * (256 * AxleCntHigh + AxleCntLow)];
            Tail[2] = message[8 + 3 * (256 * AxleCntHigh + AxleCntLow)];
            if (Tail[0] != 0x00 || Tail[1] != 0x00 || Tail[2] != 0x0D)
            {
                ValidMessage = false;
            }
        }
    }
}
