using System.Collections.Generic;

namespace SpikeCore.MessageBus
{
    public class IrcSendMessage
    {
        public IEnumerable<string> Messages { get; }

        public IrcSendMessage(string message) : this(new[] { message }) { }
        public IrcSendMessage(params string[] messages) : this((IEnumerable<string>)messages) { }
        public IrcSendMessage(IEnumerable<string> messages)
        {
            Messages = messages;
        }
    }
}