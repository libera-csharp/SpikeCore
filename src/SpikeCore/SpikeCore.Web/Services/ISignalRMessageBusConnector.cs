using System.Threading.Tasks;

namespace SpikeCore.Web.Services
{
    public interface ISignalRMessageBusConnector
    {
        Task SendMessageAsync(string message);
    }
}