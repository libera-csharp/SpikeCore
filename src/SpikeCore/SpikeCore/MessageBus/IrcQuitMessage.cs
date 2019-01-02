namespace SpikeCore.MessageBus
{
    public class IrcQuitMessage
    {
        public string Reason { get; }

        public IrcQuitMessage(string reason)
        {
            Reason = reason;
        }
    }
}