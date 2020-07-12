using System.Threading;
using System.Threading.Tasks;

using Foundatio.Messaging;

using Microsoft.AspNetCore.SignalR;

using SpikeCore.MessageBus;
using SpikeCore.Web.Hubs;

namespace SpikeCore.Web.Services
{
    public class SignalRMessageBusConnector : ISignalRMessageBusConnector, IMessageHandler<IrcReceiveMessage>
    {
        private readonly IHubContext<BotConsoleHub> _hubContext;
        private readonly IMessageBus _messageBus;

        public SignalRMessageBusConnector(IHubContext<BotConsoleHub> hubContext, IMessageBus messageBus)
        {
            _hubContext = hubContext;
            _messageBus = messageBus;
        }

        public async Task HandleMessageAsync(IrcReceiveMessage message, CancellationToken cancellationToken)
            => await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.Message, cancellationToken);

        // This should be "string channelName, string message" but UI work is being punted
        // until the React integration.
        public async Task SendMessageAsync(string channelName, string message)
            => await _messageBus.PublishAsync(new IrcSendChannelMessage(channelName, message));
    }
}