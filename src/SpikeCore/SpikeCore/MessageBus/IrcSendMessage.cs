namespace SpikeCore.MessageBus
{
    public class IrcSendMessage
    {
        public string Message { get; }
        public IrcSendMessage(string message)
        {
            Message = message;
        }
    }
}