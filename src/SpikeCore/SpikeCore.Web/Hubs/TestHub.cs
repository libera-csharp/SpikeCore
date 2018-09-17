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

        public async Task Connect()
        {
            _botManager.Connect();
        }

        public async Task SendMessage(string message)
        {
            _botManager.SendMessage(message);
            //await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}