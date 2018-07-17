using System;

namespace SpikeCore
{
    public interface IBot
    {
        Action<string> MessageRecieved { get; set; }

        void Connect();
        void SendMessage(string message);
    }
}
