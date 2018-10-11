using System;
using System.Collections.Generic;

namespace SpikeCore.Irc
{
    public abstract class IrcClientBase: IIrcClient
    {
        protected IEnumerable<string> _channelsToJoin;
        protected bool _authenticate;
        protected string _password;

        public virtual Action<string> MessageReceived { get; set; }
        public virtual Action<ChannelMessage> ChannelMessageReceived { get; set; }

        public virtual void Connect(string host, int port, string nickname, IEnumerable<string> channelsToJoin, bool authenticate, string password)
        {            
            _channelsToJoin = channelsToJoin;
            _authenticate = authenticate;
            _password = password;
        }

        public abstract void SendChannelMessage(string channelName, string message);
    }
}
