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
            _ircClient.OnChannelMessage += _ircClient_OnPrivmsg;
            _ircClient.OnQueryMessage += _ircClient_OnPrivmsg;
            _ircClient.OnQueryNotice += _ircClient_OnQueryNotice;

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
            else
            {
                // If we're not going to identify to network services, we can join channels immediately - we don't
                // expect our host to change after this point.
                JoinChannelsForNetwork();
            }
        }

        private void _ircClient_OnRawMessage(object sender, SIRC4N.IrcEventArgs e)
            => MessageReceived?.Invoke($"RAW: {e.Data.RawMessage}");

        private void _ircClient_OnPrivmsg(object sender, SIRC4N.IrcEventArgs e)
            => PrivMessageReceived?.Invoke(new PrivMessage()
            {
                ChannelName = e.Data.Channel,
                Text = e.Data.Message,
                UserHostName = e.Data.Host,
                UserName = e.Data.Nick
            });

        private void _ircClient_OnQueryNotice(object sender, SIRC4N.IrcEventArgs e)
        {
            if (_authenticate && NoticeIsExpectedServicesAgentMessage(e.Data.Nick, e.Data.Message))
            {
                JoinChannelsForNetwork();
            }            
        }
        
        public override void SendChannelMessage(string channelName, string message)
            => _ircClient.SendMessage(SIRC4N.SendType.Message, channelName, message);

        public override void SendPrivateMessage(string nick, string message)
            => _ircClient.SendMessage(SIRC4N.SendType.Message, nick, message);

        public override void JoinChannel(string channelName) 
            => _ircClient.RfcJoin(channelName);

        public override void PartChannel(string channelName, string reason)
            => _ircClient.RfcPart(channelName, reason);
        

        public override void Quit(string quitMessage)
        {
            _ircClient.RfcQuit(quitMessage ?? "Quitting...");
            WebHostCancellationTokenHolder.CancellationTokenSource.Cancel();
        }
    }
}
