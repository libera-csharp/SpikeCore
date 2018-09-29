using System.Threading.Tasks;

namespace SpikeCore.Web.Services
{
    public interface ISignalRMessageBusConnector
    {
        Task ConnectAsync();
        Task SendMessageAsync(string message);
    }
}