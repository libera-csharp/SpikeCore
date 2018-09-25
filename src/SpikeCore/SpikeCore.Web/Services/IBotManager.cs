using System.Threading.Tasks;

namespace SpikeCore.Web.Services
{
    public interface IBotManager
    {
        Task ConnectAsync();
        Task SendMessageAsync(string message);
    }
}