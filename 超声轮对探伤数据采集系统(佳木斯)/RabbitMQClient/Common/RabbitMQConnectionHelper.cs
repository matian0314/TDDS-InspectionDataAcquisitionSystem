using RabbitMQ.Client;
using System.Collections.Generic;
using System.Configuration;

namespace RabbitMQClient
{
    public class RabbitMQConnectionHelper
    {
        public static IConnection DefaultConnection { get; set; }
        /// <summary>
        /// 获取单个RabbitMQ连接
        /// </summary>
        /// <returns></returns>
        public static IConnection GetConnection()
        {
            if (DefaultConnection == null)
            {
                var factory = new ConnectionFactory
                {
                    HostName = ConfigurationManager.AppSettings["RabbitMQ_Ip"], //ip
                    Port = int.Parse(ConfigurationManager.AppSettings["RabbitMQ_Port"]), // 端口
                    UserName = ConfigurationManager.AppSettings["RabbitMQ_UserName"], // 账户
                    Password = ConfigurationManager.AppSettings["RabbitMQ_Password"], // 密码
                    VirtualHost = ConfigurationManager.AppSettings["RabbitMQ_VirtualHost"]   // 虚拟主机
                };
                DefaultConnection = factory.CreateConnection();
            }
            return DefaultConnection;
        }

        /// <summary>
        /// 根据hostName获取单个RabbitMQ连接
        /// </summary>
        /// <returns></returns>
        public static IConnection GetConnection(string hostName,int port)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName, //ip
                Port = port, // 端口
                UserName = ConfigurationManager.AppSettings["RabbitMQ_UserName"], // 账户
                Password = ConfigurationManager.AppSettings["RabbitMQ_Password"], // 密码
                VirtualHost = ConfigurationManager.AppSettings["RabbitMQ_VirtualHost"]   // 虚拟主机
            };
            return factory.CreateConnection();
        }
    }
}
