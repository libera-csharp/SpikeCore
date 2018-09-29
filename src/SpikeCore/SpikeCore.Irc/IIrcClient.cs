using System;

namespace SpikeCore.Irc
{
    public interface IIrcClient
    {
        Action<ChannelMessage> ChannelMessageReceived { get; set; }
        Action<string> MessageReceived { get; set; }

        void Connect();
        void SendMessage(string message);
    }
}
