using System;
using System.Collections.Generic;

namespace SpikeCore.Irc
{
    public abstract class IrcClientBase: IIrcClient
    {
        protected IEnumerable<string> _channelsToJoin;

        public virtual Action<string> MessageReceived { get; set; }
        public virtual Action<ChannelMessage> ChannelMessageReceived { get; set; }

        public virtual void Connect(string host, int port, string nickname, IEnumerable<string> channelsToJoin)
            => _channelsToJoin = channelsToJoin;

        public abstract void SendChannelMessage(string channelname, string message);
    }
}
