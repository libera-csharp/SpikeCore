using System;
using Microsoft.Extensions.Configuration;
using SpikeCore.Irc;
using SpikeCore.Irc.Configuration;

namespace SpikeCore
{
    public class Bot : IBot
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly object _ircClientLock = new object();
        private readonly BotConfig _botConfig = new BotConfig();
        
        private IIrcClient _ircClient;
        private Action<string> _messageReceived;

        public Action<string> MessageReceived
        {
            get
            {
                lock (_ircClientLock)
                {
                    return _messageReceived;
                }
            }
            set
            {
                lock (_ircClientLock)
                {
                    _messageReceived = value;

                    if (_ircClient != null)
                    {
                        _ircClient.MessageReceived = value;   
                    }
                }
            }
        }

        public Bot(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            configuration.GetSection("Bot").Bind(_botConfig);
        }

        public void Connect()
        {
            lock (_ircClientLock)
            {
                if (_ircClient == null)
                {
                    _ircClient = _serviceProvider.GetService(typeof(IIrcClient)) as IIrcClient;
                    
                    _ircClient.MessageReceived = _messageReceived;
                    _ircClient.Connect(_botConfig);
                }
            }
        }

        public void SendMessage(string message)
        {
            _ircClient.SendMessage(message);
        }
    }
}
