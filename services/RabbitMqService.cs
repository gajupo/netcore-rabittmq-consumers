using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rabbit_consumer.services.core;
using RabbitMQ.Client;

namespace rabbit_consumer.services
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly IConfiguration _configuration;
        public RabbitMqService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IConnection CreateChannel()
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration.GetValue<string>("RabbitConnection:Hostname"),
                UserName = _configuration.GetValue<string>("RabbitConnection:Username"),
                Password =_configuration.GetValue<string>("RabbitConnection:Password"),
                VirtualHost = _configuration.GetValue<string>("RabbitConnection:VirtualHost"),
                Port = _configuration.GetValue<int>("RabbitConnection:Port")
            };

            factory.DispatchConsumersAsync = true;

            return factory.CreateConnection();

        }
    }
}