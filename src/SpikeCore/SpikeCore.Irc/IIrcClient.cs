using System;

namespace SpikeCore.Web.Irc
{
    public interface IIrcClient
    {
        Action<string> MessageRecieved { get; set; }

        void Connect();
        void SendMessage(string message);
    }
}
