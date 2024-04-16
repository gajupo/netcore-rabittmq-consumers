using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rabbit_consumer
{
    public interface IRabbitMqSubscriber
    {
        Task Subscribe(CancellationToken cancellationToken);
    }
}