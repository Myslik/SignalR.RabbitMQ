using Microsoft.AspNet.SignalR.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.RabbitMQ
{
    public class RabbitMQMessageBus : ScaleoutMessageBus
    {
        private readonly IModel _rabbitmqchannel;
        private readonly string _rabbitmqExchangeName;
        private QueueDeclareOk _queue;
        private QueueingBasicConsumer _consumer;
        private int _resource = 0;

        private readonly Queue<Task> _publishQueue = new Queue<Task>();

        public RabbitMQMessageBus(IDependencyResolver resolver, string rabbitMqExchangeName, IModel rabbitMqChannel) : base(resolver)
        {
            _rabbitmqchannel = rabbitMqChannel;
            _rabbitmqExchangeName = rabbitMqExchangeName;

            EnsureConnection();
        }

        protected override Task Send(Message[] messages)
        {
            return Task.Factory.StartNew(msgs =>
                {
                    var typedMessages = msgs as Message[];
                    typedMessages.GroupBy(m => m.Source).ToList().ForEach(group =>
                        {
                            var source = group.Key;
                            var message = new RabbitMQMessage(group.ToArray());
                            _rabbitmqchannel.BasicPublish(_rabbitmqExchangeName, source, null, message.GetBytes());
                        });
                },
                messages);
        }

        private void EnsureConnection()
        {
            var tcs = new TaskCompletionSource<Object>();

            if (1 == Interlocked.Exchange(ref _resource, 1))
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {

                        var queue = _rabbitmqchannel.QueueDeclare("", false, false, true, null);
                        _rabbitmqchannel.QueueBind(queue.QueueName, _rabbitmqExchangeName, "#");

                        var consumer = new QueueingBasicConsumer(_rabbitmqchannel);
                        _rabbitmqchannel.BasicConsume(queue.QueueName, false, consumer);

                        while (true)
                        {
                            var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                            _rabbitmqchannel.BasicAck(ea.DeliveryTag, false);

                            var message = RabbitMQMessage.Deserialize(ea.Body);

                            OnReceived(ea.RoutingKey, ea.DeliveryTag, message.Messages);
                        }
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });
        }
    }
}
