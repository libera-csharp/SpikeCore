using System.Threading;
using System.Threading.Tasks;

using Foundatio.Messaging;

using Microsoft.AspNetCore.Identity;

using SpikeCore.Data.Models;
using SpikeCore.Irc.Configuration;
using SpikeCore.MessageBus;

namespace SpikeCore.Irc
{
    public class IrcConnection : IIrcConnection, IMessageHandler<IrcConnectMessage>, IMessageHandler<IrcSendChannelMessage>
    {
        private readonly IIrcClient _ircClient;
        private readonly IMessageBus _messageBus;
        private readonly UserManager<SpikeCoreUser> _userManager;
        private readonly IrcConnectionConfig _config;

        public IrcConnection(IIrcClient ircClient, IMessageBus messageBus, UserManager<SpikeCoreUser> userManager, IrcConnectionConfig botConfig)
        {
            _ircClient = ircClient;
            _messageBus = messageBus;
            _userManager = userManager;
            _config = botConfig;

            Connect();
        }

        public Task HandleMessageAsync(IrcConnectMessage message, CancellationToken cancellationToken)
        {
            Connect();

            return Task.CompletedTask;
        }

        public Task HandleMessageAsync(IrcSendChannelMessage ircSendMessage, CancellationToken cancellationToken)
        {
            foreach (var message in ircSendMessage.Messages)
            {
                _ircClient.SendChannelMessage(ircSendMessage.ChannelName, message);
            }

            return Task.CompletedTask;
        }

        private void Connect()
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

            _ircClient.Connect(_config.Host, _config.Port, _config.Nickname, _config.Channels, _config.Authenticate, _config.Password);
        }
    }
}