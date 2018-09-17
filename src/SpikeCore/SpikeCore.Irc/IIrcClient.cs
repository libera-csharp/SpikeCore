using System;
using SpikeCore.Irc.Configuration;

namespace SpikeCore.Irc
{
    public interface IIrcClient
    {
        Action<string> MessageReceived { get; set; }

        void Connect(BotConfig botConfig);
        void SendMessage(string message);
    }
}
