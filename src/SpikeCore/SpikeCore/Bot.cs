using System.Threading;
using System.Threading.Tasks;

using Foundatio.Messaging;

using SpikeCore.Irc;
using SpikeCore.Messages;

namespace SpikeCore
{
    public class Bot : IBot, IMessageHandler<IrcConnectMessage>, IMessageHandler<IrcSendMessage>
    {
        private readonly IIrcClient _ircClient;
        private readonly IMessageBus _messageBus;

        public Bot(IIrcClient ircClient, IMessageBus messageBus)
        {
            _ircClient = ircClient;
            _messageBus = messageBus;
        }

        public Task HandleMessageAsync(IrcConnectMessage message, CancellationToken cancellationToken)
        {
            _ircClient.MessageReceived = (receivedMessage)
                => _messageBus.PublishAsync(new IrcReceiveMessage(receivedMessage));

            _ircClient.Connect();

            return Task.CompletedTask;
        }

        public Task HandleMessageAsync(IrcSendMessage message, CancellationToken cancellationToken)
        { 
            _ircClient.SendMessage(message.Message);

            return Task.CompletedTask;
        }
    }
}