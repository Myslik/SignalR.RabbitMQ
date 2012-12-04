using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.RabbitMQ
{
    public static class DependencyResolverExtension
    {
        public static IDependencyResolver UseRabbitMQ(this IDependencyResolver resolver, string rabbitMqExchangeName, IModel rabbitMqChannel)
        {
            var bus = new Lazy<RabbitMQMessageBus>(() => new RabbitMQMessageBus(resolver, rabbitMqExchangeName, rabbitMqChannel));
            resolver.Register(typeof(IMessageBus), () => bus.Value);

            return resolver;
        }
    }
}
