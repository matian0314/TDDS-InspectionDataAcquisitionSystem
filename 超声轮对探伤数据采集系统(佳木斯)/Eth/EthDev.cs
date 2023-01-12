using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Eth
{
    public delegate void DataGeneratedHandler(ref byte[] point, string ip, int channel);
    public static class EthDev
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]//回调函数中不加这个会自动退出
        public delegate int EthCallBackFunc(ref EthRevFrame frame);

        private readonly static log4net.ILog log = log4net.LogManager.GetLogger(nameof(EthDev));
        [DllImport("ut_eth.dll")]
        public static extern int set_deal_callback(EthCallBackFunc callBack);
        [DllImport("ut_eth.dll")]
        public static extern int ut_eth_init(sbyte[] ip);
        [DllImport("ut_eth.dll")]
        public static extern int ut_eth_run();
        [DllImport("ut_eth.dll")]
        public static extern int get_connected_ip(sbyte[,] ipout, ref int len);
        [DllImport("ut_eth.dll")]
        public static extern int set_chan_cfg(sbyte[] ip, ref chan_cfg_t cfg);
        [DllImport("ut_eth.dll")]
        public static extern int set_gen_cfg(sbyte[] ip, ref gen_cfg_t cfg);
        public static EthCallBackFunc EthCallBack = new EthCallBackFunc(EthHandleFunc);

        public static event DataGeneratedHandler DataGenerated;
        public static volatile bool IsReady = false;
        private static DateTime MsgTime { get; set; } = DateTime.Now;
        public static int EthHandleFunc(ref EthRevFrame frame)
        {
            if (!IsReady)
            {
                return 0;
            }

            for (int i = 0; i < frame.len;)
            {
                unsafe
                {
                    //fixed (EthRevFrame* item = &frame)
                    //{

                    //string ip = EthHelper.IpToString(new sbyte[] { item->ip[0], item->ip[1], item->ip[2], item->ip[3] });
                    string ip = EthHelper.IpToString(new sbyte[] { frame.ip[0], frame.ip[1], frame.ip[2], frame.ip[3] });
                    if (frame.data[i] != 255)
                    {
                        log.Warn($"frame数据格式错误");
                        log.Warn($"IP地址为:{ip}");
                        //log.Warn($"i的值为{i}");
                        //log.Warn($"head: {frame.data[i]}");
                        //log.Warn($"chan: {frame.data[i + 1]}");
                        //log.Warn($"len: {frame.data[i + 2]}");
                        //log.Warn($"sign: {frame.data[i + 3]}");
                        //log.Warn($"g1_result: {frame.data[i + 4]}");
                        //log.Warn($"g2_result: {frame.data[i + 5]}");
                        //log.Warn($"resv1: {frame.data[i + 6]}");
                        //log.Warn($"g4_result: {frame.data[i + 7]}");
                        return 0;
                    }
                    int len = 4 * (frame.data[i + 2]) - 8;
                    byte[] wave = new byte[len];
                    if (len > 0)
                    {
                        for (int j = 0; j < len; j++)
                        {
                            wave[j] = frame.data[i + 8 + j];
                        }
                        DataRepo.Write(ip, frame.data[i + 1], wave);
                        DataGenerated?.Invoke(ref wave, ip, frame.data[i + 1]);
                    }
                    i += len + 8;
                    //}
                }
            }
            return 0;
        }
        /// <summary>
        /// 测试用方法
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        public static void TestEthHandler(string ip, int channel, byte[] data)
        {
            DataRepo.Write(ip, channel, data);
            DataGenerated?.Invoke(ref data, ip, channel);
        }
    }

}
