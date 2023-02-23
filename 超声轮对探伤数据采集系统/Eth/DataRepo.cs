using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using CardConfigurations;
using System.Threading.Tasks;

namespace Eth
{
    public static class DataRepo
    {
        public static log4net.ILog log = log4net.LogManager.GetLogger("DataRepo");
        public static bool WriteMemoryEnabled { get; set; } = true;
        public static string StorageDirectory { get; }
        public static string CombineFilePath { get; }
        public static Action<string> RepoWrittenComplete;
        public static Dictionary<RepoKey, List<byte[]>> Repo { get; set; } = new Dictionary<RepoKey, List<byte[]>>();
        //用来保证线程安全的锁
        private static readonly object locker = new object();
        private static Dictionary<string, int> CurrentAxle { get; set; } = new Dictionary<string, int>();
        private static readonly int ChannelPerCard = 16;
        private static DateTime FirstMessageReceiveTime;
        public static string SendDataPath = ConfigurationManager.AppSettings["SendDataPath"];
        static DataRepo()
        {
            StorageDirectory = ConfigurationManager.AppSettings["StoragePath"];
            CombineFilePath = ConfigurationManager.AppSettings["CombineFilePath"];
        }
        public static void Write(string ip, int channel, byte[] values)
        {
            if (!WriteMemoryEnabled)
            {
                return;
            }

            lock (locker)
            {
                if (FirstMessageReceiveTime == DateTime.MinValue)
                {
                    FirstMessageReceiveTime = DateTime.Now;
                }
                if (!CurrentAxle.ContainsKey(GetCurrentAxleKey(ip, channel)))
                {
                    CurrentAxle.Add(GetCurrentAxleKey(ip, channel), 1);
                }
                int axle = CurrentAxle[GetCurrentAxleKey(ip, channel)];

                var key = new RepoKey(ip, channel, axle);
                if (Repo.ContainsKey(key))
                {
                    if (Repo[key].Count < 31)
                    {
                        Repo[key].Add(values);
                    }
                    else
                    {
                        CurrentAxle[GetCurrentAxleKey(ip, channel)]++;
                        key = new RepoKey(ip, channel, axle + 1);
                        Repo[key] = new List<byte[]>() { values };
                    }
                }
                else
                {
                    Repo[key] = new List<byte[]>() { values };
                }
            }
        }
        public static void WriteToFile(CardConfigs configs)
        {
            string folderName = null;
            if (Repo.Keys.Count == 0 || Repo.Values.Sum(v => v.Count) == 0)
            {
                WriteMemoryEnabled = true;
                log.Debug($"DataRepo中无数据");
                return;
            }
            lock (locker)
            {
                WriteMemoryEnabled = false;

                try
                {
                    log.Info("开始将DataRepo写入磁盘");
                    folderName = FirstMessageReceiveTime.ToString("yyyy年MM月dd日HH时mm分ss秒");
                    Directory.CreateDirectory(Path.Combine(StorageDirectory, folderName));
                    Directory.CreateDirectory(Path.Combine(SendDataPath, ConfigurationManager.AppSettings["Side"]));

                    //数据文件 按Ip和轴统计
                    string fileName = "";
                    var orderedRepo = Repo.OrderBy(r => r.Key?.Ip).ThenBy(r => r.Key?.Axle);
                    int datFileCount = 0;
                    foreach (var item in orderedRepo)
                    {
                        //事先orderedRepo已经按IP和Axle进行过排序了，所以相同的IP和Axle是相邻的
                        if (string.IsNullOrEmpty(fileName) || fileName != item.Key.ToFileName())
                        {
                            fileName = item.Key.ToFileName();
                        }
                        string dataFileName = Path.Combine(StorageDirectory, folderName, fileName);
                        using (var sr = File.AppendText(dataFileName))
                        {
                            sr.WriteLine(JsonConvert.SerializeObject(item));
                            sr.Flush();
                            datFileCount++;
                        }
                        
                    }
                    log.Info($"生成数据文件,共{datFileCount}个");
                    //总结文件
                    string summaryFileName = Path.Combine(StorageDirectory, folderName, $"Summary{ConfigurationManager.AppSettings["Side"]}.txt");
                    var ips = Repo.Keys.Select(k => k?.Ip).Distinct();
                    using (var sr = File.AppendText(summaryFileName))
                    {
                        sr.WriteLine($"检测时间{FirstMessageReceiveTime:G}");
                        sr.WriteLine($"共收集信息{Repo.Sum(e => e.Value?.Count)}条,共包含不同Ip、通道、轴数{Repo?.Keys?.Count}组\n\r");

                        foreach (var ip in ips)
                        {
                            sr.WriteLine($"  {ip}采集数据共{Repo.Where(e => e.Key?.Ip == ip).Sum(e => e.Value?.Count)}条");
                            sr.WriteLine($"    按轴统计");
                            for (int axle = 1; axle <= Repo.Where(e => e.Key?.Ip == ip).Max(e => e.Key?.Axle); axle++)
                            {
                                sr.WriteLine($"    轴{axle}共采集数据{Repo.Where(e => e.Key?.Ip == ip && e.Key?.Axle == axle).Sum(e => e.Value?.Count)}条");
                            }
                            sr.WriteLine("    按通道统计");
                            for (int channel = 0; channel < ChannelPerCard; channel++)
                            {
                                sr.WriteLine($"    通道{channel}共采集数据{Repo.Where(e => e.Key?.Ip == ip && e.Key?.Channel == channel).Sum(e => e.Value?.Count)}条");
                            }
                            sr.WriteLine("\n\r");
                        }
                        sr.Flush();
                    }
                    log.Info($"生成过车总结文件{summaryFileName}");
                    //记录当前配置信息
                    string configFileName = Path.Combine(StorageDirectory, folderName, $"Configs{ConfigurationManager.AppSettings["Side"]}.cfg");
                    using (var sr = File.AppendText(configFileName))
                    {
                        sr.WriteLine(JsonConvert.SerializeObject(configs));
                        sr.Flush();
                    }
                    log.Info($"生成配置文件{configFileName}");
                    log.Info($"开始拷贝文件");
                    foreach (var file in Directory.GetFiles(Path.Combine(StorageDirectory, folderName)))
                    {
                        File.Copy(file, Path.Combine(CombineFilePath, Path.GetFileName(file)), true);
                        
                    }
                    //等待5s，确保前台处理完成
                    log.Debug("RepoWrittenComplete事件触发");
                    RepoWrittenComplete?.Invoke(folderName);

                    Task.Run(() =>
                    {
                        //10分钟，再发送详细数据
                        Task.Delay(TimeSpan.FromMinutes(10));
                        Directory.CreateDirectory(Path.Combine(SendDataPath, ConfigurationManager.AppSettings["Side"], folderName));
                        foreach (var file in Directory.GetFiles(Path.Combine(StorageDirectory, folderName)))
                        {
                            File.Copy(file, Path.Combine(SendDataPath, ConfigurationManager.AppSettings["Side"], folderName, Path.GetFileName(file)), true);

                        }
                    });

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    SaveDataRepo();
                }
                finally
                {
                    Repo.Clear();
                    CurrentAxle.Clear();
                    FirstMessageReceiveTime = DateTime.MinValue;
                    WriteMemoryEnabled = true;
                    GC.Collect();
                }
            }
        }
        private static string GetCurrentAxleKey(string ip, int channel)
        {
            return $"{ip}-{channel}";
        }
        #region 测试函数，正常情况下不应该调用
        public static void SaveDataRepo()
        {
            string name = $"{DateTime.Now}Restore.txt";
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            using (StreamWriter sw = File.AppendText(Path.Combine(folder, name)))
            {
                sw.WriteLine(JsonConvert.SerializeObject(DataRepo.Repo));
                sw.Flush();
            }
        }
        public static void RebuildDataRepo()
        {
            string name = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataRepo.txt");
            using (StreamReader sr = File.OpenText(name))
            {
                var content = sr.ReadLine();
                Repo = JsonConvert.DeserializeObject<Dictionary<RepoKey, List<byte[]>>>(content);
            }
        }
        #endregion
    }
}
