using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SpikeCore.Web.Services;

namespace SpikeCore.Web.Hubs
{
    public class BotConsoleHub : Hub
    {
        private readonly ISignalRMessageBusConnector _signalRMessageBusConnector;

        public BotConsoleHub(ISignalRMessageBusConnector botManager)
        {
            _signalRMessageBusConnector = botManager;
        }

        public async Task SendMessage(string channelName, string message)
        {
            await _signalRMessageBusConnector.SendMessageAsync(channelName, message);
            await Clients.All.SendAsync("ReceiveMessage", $"[Sent from Web UI to {channelName}]: {message}");
        }
    }
}