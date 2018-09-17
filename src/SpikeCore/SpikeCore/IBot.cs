using System;

namespace SpikeCore
{
    public interface IBot
    {
        Action<string> MessageReceived { get; set; }

        void Connect();
        void SendMessage(string message);
    }
}
