using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Interfaces
{
    public interface IQueueRabbitMQ
    {
        public IConnection GetRabbitConnection();
        public IModel GetRabbitChannel(IConnection conn);
        public void SendMessage(IConnection conn, string message);
    }
}
