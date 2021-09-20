using RabbitMQ.Client;
using ReportService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportService.Interfaces
{
    public interface IQueueRabbitMQ
    {
        public IConnection GetRabbitConnection();
        public IModel GetRabbitChannel(IConnection conn);
        public string ReceiveMessage(IConnection conn);
        public Task<ServiceResponseReport> Consume();
    }
}
