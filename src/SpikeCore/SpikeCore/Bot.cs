using System;
using SpikeCore.Irc;

namespace SpikeCore
{
    public class Bot : IBot
    {
        private readonly IServiceProvider serviceProvider;

        private readonly object ircClientLock = new object();
        private IIrcClient ircClient;

        private Action<string> messageReceived;

        public Action<string> MessageReceived
        {
            get
            {
                lock (ircClientLock)
                {
                    return messageReceived;
                }
            }
            set
            {
                lock (ircClientLock)
                {
                    messageReceived = value;

                    if (ircClient != null)
                    {
                        ircClient.MessageReceived = value;   
                    }
                }
            }
        }

        public Bot(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Connect()
        {
            lock (ircClientLock)
            {
                if (ircClient == null)
                {
                    ircClient = serviceProvider.GetService(typeof(IIrcClient)) as IIrcClient;
                    ircClient.MessageReceived = messageReceived;
                    ircClient.Connect();
                }
            }
        }

        public void SendMessage(string message)
        {
            ircClient.SendMessage(message);
        }
    }
}
