using System;
using System.Collections.Generic;
using System.Threading;
using SpikeCore.Domain;

namespace SpikeCore.Irc
{
    public abstract class IrcClientBase: IIrcClient
    {
        private IEnumerable<string> _channelsToJoin;
        protected string _host;
        protected int _port;
        protected string _nickname;
        protected bool _authenticate;
        protected string _password;
        protected bool _userInitiatedDisconnect;
        
        public WebHostCancellationTokenHolder WebHostCancellationTokenHolder { protected get; set; }
        public virtual Action<string> MessageReceived { get; set; }
        public virtual Action<PrivMessage> PrivMessageReceived { get; set; }
        public virtual bool IsConnected { get; }

        public virtual void Connect(string host, int port, string nickname, IEnumerable<string> channelsToJoin, bool authenticate, string password)
        {            
            _channelsToJoin = channelsToJoin;
            _authenticate = authenticate;
            _password = password;

            _host = host;
            _port = port;
            _nickname = nickname;
            
            Connect();
        }

        protected void HandleDisconnect(object sender, EventArgs e)
        {      
            UnwireEvents();
            
            if (!_userInitiatedDisconnect)
            {
                while (!IsConnected)
                {
                    Connect();

                    if (!IsConnected)
                    {
                        // TODO - [kog@epiphanic.org 01/17/2019]: replace with a max retries + exponential backoff
                        Thread.Sleep(30000);
                    }
                }
            }
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
        public abstract void PartChannel(string channelName, string reason);
        public abstract void Quit(string quitMessage);
        protected abstract void UnwireEvents();
        protected abstract void Connect();
    }
}
