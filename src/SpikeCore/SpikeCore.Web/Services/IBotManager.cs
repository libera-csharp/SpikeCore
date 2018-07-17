namespace SpikeCore.Web.Services
{
    public interface IBotManager
    {
        void Connect();
        void SendMessage(string message);
    }
}
