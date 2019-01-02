namespace SpikeCore.MessageBus
{
    public class IrcPartChannelMessage
    {
        public string Channel { get; }
        public string Reason { get; }

        public IrcPartChannelMessage(string channel, string reason)
        {
            Channel = channel;
            Reason = reason;
        }
    }
}