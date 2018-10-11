﻿using System;
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
        }

        private void LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
            => e.Channel.MessageReceived += Channel_MessageReceived;

        private void LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
            => e.Channel.MessageReceived -= Channel_MessageReceived;

        private void Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            var ircChannel = (IrcChannel)sender;

            if (ircChannel != null && e.Source is IrcUser ircUser)
            {
                ChannelMessageReceived?.Invoke(new ChannelMessage()
                {
                    ChannelName = ircChannel.Name,
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
            
            foreach (var channelToJoin in _channelsToJoin)
            {
                _ircClient.Channels.Join(channelToJoin);
            }
        }

        private void LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
            => MessageReceived?.Invoke($"{e.Source.Name}: {e.Targets}: {e.Text}");

        private void IrcClient_ConnectFailed(object sender, IrcErrorEventArgs e)
            => MessageReceived?.Invoke($"IrcClient_ConnectFailed: {e.Error.Message}");

        private void IrcClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
            => MessageReceived?.Invoke($"RAW: {e.RawContent}");
        
        public override void SendChannelMessage(string channelName, string message)
            => _ircClient.LocalUser.SendMessage(channelName, message);
    }
}
