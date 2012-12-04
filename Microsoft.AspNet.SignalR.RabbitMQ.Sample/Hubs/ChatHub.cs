using Microsoft.AspNet.SignalR.Hubs;

namespace Microsoft.AspNet.SignalR.RabbitMQ.Sample.Hubs
{
    [HubName("chat")]
    public class ChatHub : Hub
    {
        [HubMethodName("send")]
        public void Send(string message)
        {
            // Call the addMessage method on all clients
            Clients.All.addMessage(message);
        }
    }
}