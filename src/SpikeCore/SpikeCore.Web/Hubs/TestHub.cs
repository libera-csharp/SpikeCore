using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

using SpikeCore.Web.Services;

namespace SpikeCore.Web.Hubs
{
    public class TestHub : Hub
    {
        private readonly ISignalRMessageBusConnector _signalRMessageBusConnector;

        public TestHub(ISignalRMessageBusConnector botManager)
        {
            _signalRMessageBusConnector = botManager;
        }

        public async Task Connect() => await _signalRMessageBusConnector.ConnectAsync();

        public async Task SendMessage(string message)
        {
            await _signalRMessageBusConnector.SendMessageAsync(message);
            await Clients.All.SendAsync("ReceiveMessage", "[Sent from Web UI]: " + message);
        }
    }
}