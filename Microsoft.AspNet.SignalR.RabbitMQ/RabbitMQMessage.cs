using System;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.AspNet.SignalR.RabbitMQ
{
    [Serializable]
    public class RabbitMQMessage
    {
        public RabbitMQMessage(Message[] message)
        {
            Messages = message;
        }

        public Message[] Messages { get; set; }

        public byte[] GetBytes()
        {
            var s = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(s);
        }

        public static RabbitMQMessage Deserialize(byte[] data)
        {
            var s = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<RabbitMQMessage>(s);
        }
    }
}
