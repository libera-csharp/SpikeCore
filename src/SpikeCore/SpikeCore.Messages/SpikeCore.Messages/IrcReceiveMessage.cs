namespace SpikeCore.Messages
{
    public class IrcReceiveMessage
    {
        public string Message { get; }
        public IrcReceiveMessage(string message)
        {
            Message = message;
        }
    }
}