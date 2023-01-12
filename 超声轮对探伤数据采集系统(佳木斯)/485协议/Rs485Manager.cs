//using Helpers;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.IO.Ports;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace _485通信._485协议
//{
//    public static class Rs485Manager
//    {
//        public static AckSignal Ack;
//        public static StartUpSignal Start;
//        public static ShutDownSignal ShutDown;
//        public static SummarySignal Sum;
//        public static SerialPort Port;
//        public static byte[] Recv = new byte[200];
//        public static int CurrentIndex = 0;
//        public static log4net.ILog log = log4net.LogManager.GetLogger("Rs485Manager");
//        public delegate void Receive485Signal(object signal);

//        public static event Receive485Signal ShutDownSignalReceived;
//        public static event Receive485Signal StartUpSignalReceived;

//        static Rs485Manager()
//        {
//            Action rs485Thread = () =>
//            {
//                Port = new SerialPort
//                {
//                    PortName = ConfigurationManager.AppSettings["PortName"],
//                    BaudRate = 38400,
//                    DataBits = 8,
//                    Parity = 0,
//                    StopBits = StopBits.One
//                };

//                try
//                {
//                    Port.Open();
//                }
//                catch (Exception ex)
//                {

//                    log.Error(ex.ToRecord());
//                    return;
//                }
                
//                if (Port.IsOpen)
//                {
//                    log.Info($"端口开启成功,端口名{Port.PortName}, 比特率{Port.BaudRate},数据位长度{Port.DataBits},奇偶校验{Port.Parity},停止位{Port.StopBits}");
//                    while (true)
//                    {
//                        if (Port.ReadByte() != 0xF5)
//                        {
//                            continue;
//                        }
//                        else
//                        {
//                            Recv[0] = 0xF5;
//                        }
//                        Recv[1] = (byte)Port.ReadByte();
//                        if (Recv[1] <= 2)
//                        {
//                            continue;
//                        }
//                        for (int i = 2; i < Recv[1]; i++)
//                        {
//                            Recv[i] = (byte)Port.ReadByte();
//                        }
//                        if (Recv[2] == 0xF0)
//                        {
//                            log.Debug("收到485应答信号");
//                            Ack = new AckSignal(Recv.Take(7).ToArray());
//                            log.Debug(JsonConvert.SerializeObject(Ack));
//                        }
//                        else if (Recv[2] == 0x5F)
//                        {
//                            log.Debug("收到485开机信号");
//                            Start = new StartUpSignal(Recv.Take(7).ToArray());
//                            log.Debug(JsonConvert.SerializeObject(Start));
//                            if (Start.ValidMessage)
//                            {
//                                log.Debug("触发开机事件");
//                                StartUpSignalReceived?.Invoke(Start);
//                            }
//                        }
//                        else if (Recv[2] == 0x6F)
//                        {
//                            log.Debug("收到485关机信号");
//                            ShutDown = new ShutDownSignal(Recv.Take(7).ToArray());
//                            log.Debug(JsonConvert.SerializeObject(ShutDown));
//                            if (ShutDown.ValidMessage)
//                            {
//                                log.Debug("触发关机事件");
//                                ShutDownSignalReceived?.Invoke(ShutDown);
//                            }
//                        }
//                        else if (Recv[2] == 0xAF)
//                        {
//                            log.Debug("收到485总结信号");
//                            Sum = new SummarySignal(Recv.Take(Recv[1]).ToArray());
//                            log.Debug(JsonConvert.SerializeObject(Sum));
//                        }
//                    }
//                }
//                else
//                {
//                    log.Info("端口开启失败");
//                }
//            };
//            Task.Run(rs485Thread);
//        }
//    }
//}
