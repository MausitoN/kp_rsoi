using BookingService.Interfaces;
using BookingService.Models;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookingService.Services
{
    public class QueueRabbitMQ: IQueueRabbitMQ
    {
        public readonly string exchangeName = "test";
        public readonly string queueName = "test";
        public readonly string routingKey = "test";
        public readonly IServiceProvider _provide;

        public QueueRabbitMQ(IServiceProvider provide)
        {
            _provide = provide;
        }

        public IConnection GetRabbitConnection()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "vrnyjpip";
            factory.Password = "i851CKQMh_xjind3Xq0IYVWjdT2A76EX";
            factory.VirtualHost = "vrnyjpip";
            factory.HostName = "bonobo-01.rmq.cloudamqp.com";
            IConnection conn = factory.CreateConnection();
            return conn;
        }

        public IModel GetRabbitChannel(IConnection conn)
        {
            IModel channel = conn.CreateModel();
            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);
            return channel;
        }

        public void SendMessage(IConnection conn, string message)
        {
            IModel model = GetRabbitChannel(conn);
            byte[] messageBodyBytes = Encoding.UTF8.GetBytes(message);
            model.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);
        }
    }
}
