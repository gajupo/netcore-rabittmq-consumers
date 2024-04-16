using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace rabbit_consumer.services.core
{
    public interface IRabbitMqService
    {
        IConnection CreateChannel();
    }
}