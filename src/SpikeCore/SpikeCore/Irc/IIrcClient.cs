using System;
using System.Collections.Generic;

namespace SpikeCore.Irc
{
    public interface IIrcClient
    {
        Action<ChannelMessage> ChannelMessageReceived { get; set; }
        Action<string> MessageReceived { get; set; }

        void Connect(string host, int port, string nickname, IEnumerable<string> channelsToJoin, bool authenticate, string password);
        void SendChannelMessage(string channelName, string message);
    }
}
