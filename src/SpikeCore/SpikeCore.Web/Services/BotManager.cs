using System;

using Microsoft.AspNetCore.SignalR;

using SpikeCore.Web.Hubs;

namespace SpikeCore.Web.Services
{
    public class BotManager : IBotManager
    {
        private readonly IBot _bot;
        private readonly IHubContext<TestHub> _hubContext;

        public BotManager(IBot bot, IHubContext<TestHub> hubContext)
        {
            _bot = bot;
            _hubContext = hubContext;
        }

        public void Connect()
        {
            _bot.MessageReceived = (message) => _hubContext.Clients.All.SendAsync("ReceiveMessage", message).Wait();
            _bot.Connect();
        }

        public void SendMessage(string message)
        {
            _bot.SendMessage(message);
        }
    }
}
