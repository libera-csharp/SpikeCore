using System;

using SpikeCore.Web.Irc;

namespace SpikeCore
{
    public class Bot : IBot
    {
        private readonly IServiceProvider serviceProvider;

        private readonly object ircClientLock = new object();
        private IIrcClient ircClient;

        private Action<string> messageRecieved;

        public Action<string> MessageRecieved
        {
            get
            {
                lock (ircClientLock)
                {
                    return messageRecieved;
                }
            }
            set
            {
                lock (ircClientLock)
                {
                    messageRecieved = value;

                    if (ircClient != null)
                        ircClient.MessageRecieved = value;
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
                    ircClient.MessageRecieved = messageRecieved;
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
