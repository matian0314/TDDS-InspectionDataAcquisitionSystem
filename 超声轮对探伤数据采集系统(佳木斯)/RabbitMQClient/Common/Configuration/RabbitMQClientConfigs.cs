using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Threading;
using RabbitMQClient;
using System.Configuration;
using Newtonsoft.Json;
using log4net;
using Tools;

namespace 报文转发.RabbitMQClient.Common.Configuration
{
    public class RabbitMQClientConfigs
    {
        //STATIC Member
        //Public
        public static string DefaultSendBackupDirectory = ConfigurationManager.AppSettings["DefaultSendBackupDirectory"];
        public static string DefaultReceiveBackupDirectory = ConfigurationManager.AppSettings["DefaultReceiveBackupDirectory"];
        //Private
        private static RabbitMQClientConfigs Config { get; set; } = null;
        private static object locker = new object();
        private static ILog log = LogManager.GetLogger("RabbitMQClientConfigs");
        //COMMON Member
        //public
        public List<RabbitMQSendFileConfig> SendFileConfigurations { get; set; } = new List<RabbitMQSendFileConfig>();
        public List<RabbitMQReceiveFileConfig> ReceiveFileConfigurations { get; set; } = new List<RabbitMQReceiveFileConfig>();
        //private
        private Dictionary<string, List<string>> FileToSendDict = new Dictionary<string, List<string>>();
        //STATIC Method
        public static RabbitMQClientConfigs GetRabbitMQClientConfigs()
        {
            // 多线程下，采用double-check保证单例模式，先检查是否为null
            if (Config == null)
            {
                // 使用lock
                lock (locker)
                {
                    if (Config != null)
                    {
                        return Config;
                    }
                    else
                    {
                        string xmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "RabbitMQClient", "client.config");
                        Config = XmlHelper.FileToObject<RabbitMQClientConfigs>(xmlPath);
                        return Config;
                    }
                }
            }
            return Config;
        }
        public RabbitMQClientConfigs() { }
        public void Run()
        {

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {

                        foreach (var config in SendFileConfigurations)
                        {
                            //如果Directory不存在，则创建一个
                            Directory.CreateDirectory(config.Directory);
                            var files = new DirectoryInfo(config.Directory).GetFiles(config.SearchPattern, SearchOption.AllDirectories).OrderBy(f => f.CreationTime).ToList();
                            if (files.Count == 0) continue;
                            else
                            {
                                if (FileToSendDict.ContainsKey(config.Key))
                                {
                                    FileToSendDict[config.Key].AddRange(files.Select(f => f.FullName));
                                }
                                else
                                {
                                    FileToSendDict.Add(config.Key, files.Select(f => f.FullName).ToList());
                                }
                            }

                        }
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                        foreach (var key in FileToSendDict.Keys)
                        {
                            foreach (var file in FileToSendDict[key])
                            {
                                log.Info($"发送文件{file},标识为{key}");
                                var rabbitFile = RabbitMQFile.Create(file, SendFileConfigurations.First(c => c.Key == key));
                                if (RabbitMQProvider.SendMessage(key, JsonConvert.SerializeObject(rabbitFile)))
                                {//发送成功，如果字典中没有其他记录，则删除文件
                                    File.Delete(file);
                                }
                                else
                                {
                                    MoveSendFile(file, SendFileConfigurations.First(s => s.Key == key).BackupDirectory);
                                }
                            }
                        }
                        FileToSendDict.Clear();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.ToRecord());
                        FileToSendDict.Clear();
                        Thread.Sleep(TimeSpan.FromSeconds(30));
                    }
                }
            });
            foreach (var config in ReceiveFileConfigurations)
            {
                RabbitMQConsumer.StartReceiveMessage(config.Key, (content) =>
                {
                    OnReceiveFile(content);
                });
            }

        }

        private void MoveSendFile(string fileName, string BackupDirectory)
        {
            if (string.IsNullOrWhiteSpace(BackupDirectory))
            {
                BackupDirectory = DefaultSendBackupDirectory;
            }
            string file = Path.GetFileName(fileName);
            File.Move(fileName, Path.Combine(BackupDirectory, file));
        }
        public void OnReceiveFile(string content)
        {
            var file = JsonConvert.DeserializeObject<RabbitMQFile>(content);
            log.Info($"收到{file.Key}报文:{file.Name}");
            var config = ReceiveFileConfigurations.First(r => r.Key == file.Key);
            string fileFullPath;
            if (file.MD5Valid())
            {
                fileFullPath = Path.Combine(config.Directory, file.Directory, file.Name);
            }
            else
            {
                if (string.IsNullOrEmpty(config.BackupDirectory))
                {
                    fileFullPath = Path.Combine(DefaultReceiveBackupDirectory ?? "", file.Directory, file.Name);
                }
                else
                {
                    fileFullPath = Path.Combine(config.BackupDirectory, file.Directory, file.Name);
                }
            }
            FileHelper.CreateFile(fileFullPath, file.Content);
            log.Info($"将{file.Key}报文存储到{fileFullPath}");
        }
    }
}
