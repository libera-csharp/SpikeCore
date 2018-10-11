using System;
using System.Collections.Generic;
using System.Threading;

using SIRC4N = Meebey.SmartIrc4net;

namespace SpikeCore.Irc.Irc4NetButSmarter
{
    public class IrcClient : IrcClientBase
    {
        private SIRC4N.IrcClient _ircClient = new SIRC4N.IrcClient();
        private Thread _listenThread;

        public override void Connect(string host, int port, string nickname, IEnumerable<string> channelsToJoin, bool authenticate, string password)
        {
            _ircClient = new SIRC4N.IrcClient();
            _ircClient.OnRawMessage += _ircClient_OnRawMessage;
            _ircClient.OnRegistered += _ircClient_OnRegistered;
            _ircClient.OnChannelMessage += _ircClient_OnChannelMessage;

            base.Connect(host, port, nickname, channelsToJoin, authenticate, password);

            _ircClient.Connect(host, port);
            _ircClient.Login(nickname, nickname);

            _listenThread = new Thread(_ircClient.Listen);
            _listenThread.Start();
        }

        private void _ircClient_OnRegistered(object sender, EventArgs e)
        {
            if (_authenticate)
            {
                _ircClient.SendMessage(SIRC4N.SendType.Message, "nickserv", $"identify {_password}");
            }           
            
            foreach (var channelToJoin in _channelsToJoin)
            {
                _ircClient.RfcJoin(channelToJoin);
            }
        }

        private void _ircClient_OnRawMessage(object sender, SIRC4N.IrcEventArgs e)
            => MessageReceived?.Invoke($"RAW: {e.Data.RawMessage}");

        private void _ircClient_OnChannelMessage(object sender, SIRC4N.IrcEventArgs e)
            => ChannelMessageReceived?.Invoke(new ChannelMessage()
            {
                ChannelName = e.Data.Channel,
                Text = e.Data.Message,
                UserHostName = e.Data.Host,
                UserName = e.Data.Nick
            });

        public override void SendChannelMessage(string channelName, string message)
            => _ircClient.SendMessage(SIRC4N.SendType.Message, channelName, message);
    }
}
