using Foundatio.Messaging;
using SpikeCore.Irc;
using SpikeCore.Messages;

namespace SpikeCore
{
    public class Bot : IBot
    {
        public Bot(IIrcClient ircClient, IMessageBus messageBus)
        {
            messageBus.SubscribeAsync<IrcSendMessage>(message => ircClient.SendMessage(message.Message));

            messageBus.SubscribeAsync<IrcConnectMessage>(connectMessage =>
            {
                ircClient.MessageReceived = (receivedMessage) =>messageBus.PublishAsync(new IrcReceiveMessage(receivedMessage));
                ircClient.Connect();
            });
        }
    }
}