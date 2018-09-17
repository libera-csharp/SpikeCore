using System;

using IrcDotNet;

namespace SpikeCore.Irc.IrcDotNet
{
    public class IrcClient : IIrcClient
    {
        private StandardIrcClient _ircClient;
        public Action<string> MessageReceived { get; set; }

        public void Connect()
        {
            _ircClient = new StandardIrcClient();
            _ircClient.RawMessageReceived += IrcClient_RawMessageReceived;

            _ircClient.Connected += IrcClient_Connected;
            _ircClient.ConnectFailed += IrcClient_ConnectFailed;

            _ircClient.Connect("chat.freenode.net", 6667, false, new IrcUserRegistrationInfo()
            {
                NickName = "SpikeCore",
                RealName = "SpikeCore",
                UserName = "SpikeCore",
            });
        }

        private void IrcClient_Connected(object sender, EventArgs e)
        {
            _ircClient.LocalUser.MessageReceived += LocalUser_MessageReceived;
            _ircClient.Channels.Join("#spikelite");
        }

        private void LocalUser_MessageReceived(object sender, IrcMessageEventArgs e) => MessageReceived?.Invoke($"{e.Source.Name}: {e.Targets}: {e.Text}");
        private void IrcClient_ConnectFailed(object sender, IrcErrorEventArgs e) => MessageReceived?.Invoke($"IrcClient_ConnectFailed: {e.Error.Message}");
        private void IrcClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e) => MessageReceived?.Invoke($"RAW: {e.RawContent}");
        public void SendMessage(string message) => _ircClient.LocalUser.SendMessage("#spikelite", message);
    }
}
