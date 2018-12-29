using System;
using System.Collections.Generic;

using IrcDotNet;

namespace SpikeCore.Irc.IrcDotNet
{
    public class IrcClient : IrcClientBase
    {
        private StandardIrcClient _ircClient;

        public override void Connect(string host, int port, string nickname, IEnumerable<string> channelsToJoin, bool authenticate, string password)
        {
            base.Connect(host, port, nickname, channelsToJoin, authenticate, password);

            _ircClient = new StandardIrcClient();
            _ircClient.RawMessageReceived += IrcClient_RawMessageReceived;

            _ircClient.Connected += IrcClient_Connected;
            _ircClient.ConnectFailed += IrcClient_ConnectFailed;
            _ircClient.Registered += _ircClient_Registered;

            _ircClient.Connect(host, port, false, new IrcUserRegistrationInfo()
            {
                NickName = nickname,
                RealName = nickname,
                UserName = nickname,
            });
        }

        private void _ircClient_Registered(object sender, EventArgs e)
        {
            _ircClient.LocalUser.JoinedChannel += LocalUser_JoinedChannel;
            _ircClient.LocalUser.LeftChannel += LocalUser_LeftChannel;
            _ircClient.LocalUser.NoticeReceived += LocalUser_NoticeReceived;
            _ircClient.LocalUser.MessageReceived += Privmsg_MessageReceived;
        }

        private void LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
            => e.Channel.MessageReceived += Privmsg_MessageReceived;

        private void LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
            => e.Channel.MessageReceived -= Privmsg_MessageReceived;

        private void Privmsg_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            var ircChannel = sender as IrcChannel;

            if (e.Source is IrcUser ircUser)
            {
                PrivMessageReceived?.Invoke(new PrivMessage()
                {
                    ChannelName = ircChannel?.Name,
                    UserName = ircUser.NickName,
                    UserHostName = ircUser.HostName,
                    Text = e.Text
                });
            }
        }

        private void IrcClient_Connected(object sender, EventArgs e)
        {
            _ircClient.LocalUser.MessageReceived += LocalUser_MessageReceived;

            if (_authenticate)
            {
                _ircClient.LocalUser.SendMessage("nickserv", $"identify {_password}");
            }
            else
            {
                // If we're not going to identify to network services, we can join channels immediately - we don't
                // expect our host to change after this point.
                JoinChannelsForNetwork();
            }
        }

        private void LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
            => MessageReceived?.Invoke($"{e.Source.Name}: {e.Targets}: {e.Text}");

        private void IrcClient_ConnectFailed(object sender, IrcErrorEventArgs e)
            => MessageReceived?.Invoke($"IrcClient_ConnectFailed: {e.Error.Message}");

        private void IrcClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
            => MessageReceived?.Invoke($"RAW: {e.RawContent}");
        
        private void LocalUser_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            var user = e.Source as IrcUser;
            
            if (_authenticate && NoticeIsExpectedServicesAgentMessage(user.NickName, e.Text))
            {
                JoinChannelsForNetwork();
            }
        }
        
        public override void SendChannelMessage(string channelName, string message)
            => _ircClient.LocalUser.SendMessage(channelName, message);

        public override void SendPrivateMessage(string nick, string message) 
            => _ircClient.LocalUser.SendMessage(nick, message);

        public override void JoinChannel(string channelName) 
            => _ircClient.Channels.Join(channelName);    
    }
}
