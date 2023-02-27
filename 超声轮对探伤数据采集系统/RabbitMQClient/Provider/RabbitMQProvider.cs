using Helpers;
using log4net;
using MyLogger;
using RabbitMQ.Client;
using System;
using System.Configuration;
using System.Text;

namespace RabbitMQClient
{
    public static class RabbitMQProvider
    {
        private static readonly SubscribeLogger log = SubscribeLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static bool SendMessage(string key, string message)
        {
            if (!string.Equals(ConfigurationManager.AppSettings["EnalbeRabbitMq"], "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            IModel channel = null;
            try
            {
                var conn = RabbitMQConnectionHelper.GetConnection();
                {
                    channel = conn.CreateModel();
                    {
                        channel.ExchangeDeclare(key, ExchangeType.Direct, true);
                        channel.QueueDeclare(key, true, false, false, null);
                        channel.QueueBind(key, key, "", null);
                        var properties = channel.CreateBasicProperties();
                        properties.DeliveryMode = 2;
                        properties.Persistent = true;
                        byte[] byteMessage = Encoding.UTF8.GetBytes(message);
                        return NormalConfirm(key, channel, properties, byteMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord());
                channel?.Close();
                return false;
            }

        }
        private static bool NormalConfirm(string key, IModel channel, IBasicProperties properties, byte[] message)
        {
            channel.ConfirmSelect(); // 开启消息确认模式
            channel.BasicPublish(key, "", properties, message);
            // 消息到达服务端队列中才返回结果
            if (!channel.WaitForConfirms())
            {
                log.Error($"{key}发送失败");
                channel.Close();
                return false;
            }
            else
            {
                log.Info($"{key}发送成功");
                channel.Close();
                return true;
            }
        }
    }
}
