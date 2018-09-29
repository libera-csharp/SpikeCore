using System.Threading;
using System.Threading.Tasks;

using Foundatio.Messaging;

using Microsoft.AspNetCore.Identity;

using SpikeCore.Data.Models;
using SpikeCore.MessageBus;

namespace SpikeCore.Irc
{
    public class IrcConnection : IIrcConnection, IMessageHandler<IrcConnectMessage>, IMessageHandler<IrcSendMessage>
    {
        private readonly IIrcClient _ircClient;
        private readonly IMessageBus _messageBus;
        private readonly UserManager<SpikeCoreUser> _userManager;

        public IrcConnection(IIrcClient ircClient, IMessageBus messageBus, UserManager<SpikeCoreUser> userManager)
        {
            _ircClient = ircClient;
            _messageBus = messageBus;
            _userManager = userManager;
        }

        public Task HandleMessageAsync(IrcConnectMessage message, CancellationToken cancellationToken)
        {
            _ircClient.ChannelMessageReceived = async (channelMessage) =>
            {
                var user = await _userManager.FindByLoginAsync("IrcHost", channelMessage.UserHostName);

                var ircChannelMessageMessage = new IrcChannelMessageMessage()
                {
                    ChannelName = channelMessage.ChannelName,
                    UserName = channelMessage.UserName,
                    UserHostName = channelMessage.UserHostName,
                    Text = channelMessage.Text,
                    IdentityUser = user
                };

                await _messageBus.PublishAsync(ircChannelMessageMessage);
            };

            _ircClient.MessageReceived = (receivedMessage) => _messageBus.PublishAsync(new IrcReceiveMessage(receivedMessage));

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