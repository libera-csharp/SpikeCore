using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

using SpikeCore.Web.Services;

namespace SpikeCore.Web.Hubs
{
    public class TestHub : Hub
    {
        private readonly IBotManager botManager;

        public TestHub(IBotManager botManager)
        {
            this.botManager = botManager;
        }

        public async Task Connect()
        {
            this.botManager.Connect();
        }

        public async Task SendMessage(string message)
        {
            this.botManager.SendMessage(message);
            //await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}