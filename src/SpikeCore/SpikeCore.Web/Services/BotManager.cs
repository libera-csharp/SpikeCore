using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.AspNetCore.SignalR;
using SpikeCore.Messages;
using SpikeCore.Web.Hubs;

namespace SpikeCore.Web.Services
{
    public class BotManager : IBotManager
    {
        private readonly IMessageBus _messageBus;

        public BotManager(IHubContext<TestHub> hubContext, IMessageBus messageBus)
        {
            _messageBus = messageBus;
            _messageBus.SubscribeAsync<IrcReceiveMessage>(message => hubContext.Clients.All.SendAsync("ReceiveMessage", message.Message));
        }

        public async Task ConnectAsync() => await _messageBus.PublishAsync(new IrcConnectMessage());
        public async Task SendMessageAsync(string message) => await _messageBus.PublishAsync(new IrcSendMessage(message));
    }
}