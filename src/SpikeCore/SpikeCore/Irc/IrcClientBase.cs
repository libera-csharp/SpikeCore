using System;
using System.Collections.Generic;

namespace SpikeCore.Irc
{
    public abstract class IrcClientBase: IIrcClient
    {
        private IEnumerable<string> _channelsToJoin;
        protected bool _authenticate;
        protected string _password;

        public virtual Action<string> MessageReceived { get; set; }
        public virtual Action<PrivMessage> PrivMessageReceived { get; set; }

        public virtual void Connect(string host, int port, string nickname, IEnumerable<string> channelsToJoin, bool authenticate, string password)
        {            
            _channelsToJoin = channelsToJoin;
            _authenticate = authenticate;
            _password = password;
        }

        protected static bool NoticeIsExpectedServicesAgentMessage(string nickname, string notice)
        {
            if ("nickserv".Equals(nickname, StringComparison.OrdinalIgnoreCase))
            {
                return notice.StartsWith("You are now identified for", StringComparison.OrdinalIgnoreCase) ||
                       (notice.StartsWith("The nickname", StringComparison.OrdinalIgnoreCase) && notice.EndsWith("is not registered", StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }
        
        protected void JoinChannelsForNetwork()
        {            
            foreach (var channel in _channelsToJoin)
            {
                JoinChannel(channel);
            }
        }
        
        public abstract void SendChannelMessage(string channelName, string message);
        public abstract void SendPrivateMessage(string nick, string message);
        public abstract void JoinChannel(string channelName);
    }
}
