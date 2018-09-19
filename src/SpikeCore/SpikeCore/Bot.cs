using System;
using SpikeCore.Irc;

namespace SpikeCore
{
    public class Bot : IBot
    {
        private readonly IIrcClient _ircClient;
        private readonly object _ircClientLock = new object();
        
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

        public Bot(IIrcClient ircClient)
        {
            _ircClient = ircClient;
        }

        public void Connect()
        {                 
            _ircClient.MessageReceived = _messageReceived;
            _ircClient.Connect();
        }

        public void SendMessage(string message)
        {
            _ircClient.SendMessage(message);
        }
    }
}
