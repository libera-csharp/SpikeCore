using System.Threading;
using System.Threading.Tasks;

using Foundatio.Messaging;

using Microsoft.AspNetCore.Identity;

using SpikeCore.Data.Models;
using SpikeCore.Irc.Configuration;
using SpikeCore.MessageBus;

namespace SpikeCore.Irc
{
    public class IrcConnection : IIrcConnection, IMessageHandler<IrcConnectMessage>, IMessageHandler<IrcSendChannelMessage>, IMessageHandler<IrcSendPrivateMessage>
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

        public Task HandleMessageAsync(IrcSendPrivateMessage ircSendPrivateMessage, CancellationToken cancellationToken)
        {
            foreach (var message in ircSendPrivateMessage.Messages)
            {
                _ircClient.SendPrivateMessage(ircSendPrivateMessage.Nick, message);
            }

            return Task.CompletedTask;
        }

        private void Connect()
        {
            _ircClient.PrivMessageReceived = async (channelMessage) =>
            {
                var message = new IrcPrivMessage()
                {
                    ChannelName = channelMessage.ChannelName,
                    UserName = channelMessage.UserName,
                    UserHostName = channelMessage.UserHostName,
                    Text = channelMessage.Text,
                    IdentityUser = await FindSpikeCoreUser(channelMessage)
                };

                await _messageBus.PublishAsync(message);
            };

            _ircClient.MessageReceived = (receivedMessage) => _messageBus.PublishAsync(new IrcReceiveMessage(receivedMessage));

            _ircClient.Connect(_config.Host, _config.Port, _config.Nickname, _config.Channels, _config.Authenticate, _config.Password);
        }

        private async Task<SpikeCoreUser> FindSpikeCoreUser(PrivMessage privMessage)
        {
            var user = await _userManager.FindByLoginAsync("IrcHost", privMessage.UserHostName);

            if (null != user)
            {
                user.Roles = await _userManager.GetRolesAsync(user);
            }

            return user;
        }
    }
}