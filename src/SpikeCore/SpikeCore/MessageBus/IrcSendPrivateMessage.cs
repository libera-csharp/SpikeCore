using System.Collections.Generic;

namespace SpikeCore.MessageBus
{
    public class IrcSendPrivateMessage
    {
        public string Nick { get; }
        public IEnumerable<string> Messages { get; }

        public IrcSendPrivateMessage(string nick, string message) : this(nick, new[] { message }) { }
        public IrcSendPrivateMessage(string nick, IEnumerable<string> messages)
        {
            Nick = nick;
            Messages = messages;
        }
    }
}