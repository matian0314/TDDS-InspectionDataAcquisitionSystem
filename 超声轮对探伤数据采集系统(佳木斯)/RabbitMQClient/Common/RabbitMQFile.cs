using log4net;
using System.IO;
using Tools;
using 报文转发.RabbitMQClient.Common.Configuration;

namespace 报文转发.RabbitMQClient.Common
{
    public class RabbitMQFile
    {
        public string Key { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 发送或存储时,在根文件夹下的子文件夹名
        /// 如果需要在存储时也保留的，放在Directory中
        /// </summary>
        public string Directory { get; set; }
        public byte[] Content { get; set; }
        public string MD5 { get; set; }
        /// <summary>
        /// 文件最后修改时间
        /// </summary>
        public string CreationTime { get; set; }
        private static ILog log = LogManager.GetLogger("RabbitMQFile");
        private RabbitMQFile() { }
        public static RabbitMQFile Create(string fullName, RabbitMQSendFileConfig config)
        {
            fullName = Path.GetFullPath(fullName);
            RabbitMQFile file = new RabbitMQFile()
            {
                Key = config.Key,
                Name = Path.GetFileName(fullName)
            };
            if (!File.Exists(fullName) || !FileHelper.FileCanRead(fullName))
            {
                log.Warn($"无法读取文件{fullName}");
                file.Content = new byte[] { };
            }
            else
            {
                using (FileStream fs = File.OpenRead(fullName))
                {
                    int length = (int)fs.Length;
                    file.Content = new byte[length];
                    fs.Read(file.Content, 0, length);
                }
            }
            if (!config.KeepDirectory)
            {
                file.Directory = "";
            }
            else
            {
                string dir = Path.GetDirectoryName(fullName);
                var baseDirLength = Path.GetFullPath(config.Directory).Length;
                file.Directory = dir.Substring(baseDirLength).TrimStart('\\');
            }
            file.MD5 = file.GetMD5();
            return file;
        }
        public string GetMD5()
        {
            return (Key + Name + Directory + Content).MD5Encrpt();
        }
        public bool MD5Valid()
        {
            return MD5 == GetMD5();
        }
        public void SaveFile(RabbitMQReceiveFileConfig config)
        {
            string dir;
            if(MD5Valid())
            {
                dir = Path.GetFullPath(config.Directory);
            }
            else
            {//发送到备用文件夹
                dir = Path.GetFullPath(config.BackupDirectory);
            }
            using(FileStream fs = new FileStream(Path.Combine(dir, this.Directory, this.Name), FileMode.Create, FileAccess.ReadWrite))
            {
                fs.Write(this.Content, 0, this.Content.Length);
                fs.Flush();
            }
        }
    }
}
