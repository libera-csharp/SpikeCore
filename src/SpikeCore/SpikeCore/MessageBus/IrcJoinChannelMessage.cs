namespace SpikeCore.MessageBus
{
    public class IrcJoinChannelMessage
    {
        public string Channel { get; }

        public IrcJoinChannelMessage(string channel)
        {
            Channel = channel;
        }
    }
}