using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Configuration;
using System.Text;
using System.Threading;
using Tools;

namespace RabbitMQClient
{
    public static class RabbitMQConsumer
    {
        private static ILog log = LogManager.GetLogger("RabbitMQConsumer");
        public static void StartReceiveMessage(string key, Action<string> OnReceiveMessage)
        {
            if(!string.Equals(ConfigurationManager.AppSettings["EnalbeRabbitMq"], "true", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            try
            {
                var connection = RabbitMQConnectionHelper.GetConnection();
                {
                    var channel = connection.CreateModel();
                    {
                        channel.ExchangeDeclare(key, ExchangeType.Direct, true);
                        channel.QueueDeclare(key, true, false, false, null);
                        channel.QueueBind(key, key, "", null);

                        //回调，当consumer收到消息后会执行该函数
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            log.Info($"收到来自{key}的消息");
                            try
                            {
                                OnReceiveMessage?.Invoke(message);
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex.ToRecord());
                            }
                            finally
                            {
                                channel.BasicAck(ea.DeliveryTag, false);
                            }

                        };
                        channel.BasicConsume(queue: key,
                                             autoAck: false,
                                             consumer: consumer);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord());
                Thread.Sleep(TimeSpan.FromSeconds(10));
                StartReceiveMessage(key, OnReceiveMessage);
            }
        }
    }
}
