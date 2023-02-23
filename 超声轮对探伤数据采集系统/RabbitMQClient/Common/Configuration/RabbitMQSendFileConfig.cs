using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 报文转发.RabbitMQClient.Common.Configuration
{
    public class RabbitMQSendFileConfig
    {
        public string Key { get; set; }
        public string Directory { get; set; }
        public string SearchPattern { get; set; }
        public bool KeepDirectory { get; set; }
        /// <summary>
        /// 如果发送失败，将会被转移到哪个文件夹
        /// </summary>
        public string BackupDirectory { get; set; }
    }
}
