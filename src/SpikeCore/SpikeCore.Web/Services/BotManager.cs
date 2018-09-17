using System;

using Microsoft.AspNetCore.SignalR;

using SpikeCore.Web.Hubs;

namespace SpikeCore.Web.Services
{
    public class BotManager : IBotManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<TestHub> _hubContext;
        private readonly object _botLock = new object();
        private IBot _bot;

        public BotManager(IServiceProvider serviceProvider, IHubContext<TestHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
        }

        public void Connect()
        {
            lock (_botLock)
            {
                if (_bot == null)
                {
                    _bot = _serviceProvider.GetService(typeof(IBot)) as IBot;
                    _bot.MessageReceived = (message) => _hubContext.Clients.All.SendAsync("ReceiveMessage", message).Wait();
                    _bot.Connect();
                }
            }
        }

        public void SendMessage(string message)
        {
            _bot.SendMessage(message);
        }
    }
}
