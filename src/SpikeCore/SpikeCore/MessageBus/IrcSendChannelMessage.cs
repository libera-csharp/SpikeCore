using System.Collections.Generic;

namespace SpikeCore.MessageBus
{
    public class IrcSendChannelMessage
    {
        public string ChannelName { get; }
        public IEnumerable<string> Messages { get; }

        public IrcSendChannelMessage(string channelname, string message) : this(channelname, new[] { message }) { }
        public IrcSendChannelMessage(string channelname, IEnumerable<string> messages)
        {
            ChannelName = channelname;
            Messages = messages;
        }
    }
}