using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

using SpikeCore.Web.Services;

namespace SpikeCore.Web.Hubs
{
    public class TestHub : Hub
    {
        private readonly IBotManager _botManager;

        public TestHub(IBotManager botManager)
        {
            _botManager = botManager;
        }

        public async Task Connect() => await _botManager.ConnectAsync();

        public async Task SendMessage(string message)
        {
            await _botManager.SendMessageAsync(message);
            await Clients.All.SendAsync("ReceiveMessage", "[Sent from Web UI]: " + message);
        }
    }
}