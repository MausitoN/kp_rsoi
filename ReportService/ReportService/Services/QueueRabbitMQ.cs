using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using ReportService.Interfaces;
using ReportService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReportService.Services
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

        public string ReceiveMessage(IConnection conn)
        {
            IModel model = GetRabbitChannel(conn);
            BasicGetResult result = model.BasicGet(queueName, false);
            if (result == null)
            {
                return null;
            }
            else
            {
                byte[] body = result.Body.ToArray();
                model.BasicAck(result.DeliveryTag, false);
                return Encoding.UTF8.GetString(body);
            }
        }

        public async Task<ServiceResponseReport> Consume()
        {
            IConnection conn = GetRabbitConnection();
            IModel channel = GetRabbitChannel(conn);

            var message = ReceiveMessage(conn);
            while (message != null)
            {
                ReportRequest item = JsonSerializer.Deserialize<ReportRequest>(message);
                using var scope = _provide.CreateScope();
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                await reportService.AddReport(new ReportRequest { Model = item.Model, Location = item.Location });
                message = ReceiveMessage(conn);
            }

            channel.Close();
            conn.Close();

            return new ServiceResponseReport(200);
        }
    }
}
