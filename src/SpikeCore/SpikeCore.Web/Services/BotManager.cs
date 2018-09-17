using System;

using Microsoft.AspNetCore.SignalR;

using SpikeCore.Web.Hubs;

namespace SpikeCore.Web.Services
{
    public class BotManager : IBotManager
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHubContext<TestHub> hubContext;

        private readonly object botLock = new object();
        private IBot bot;

        public BotManager(IServiceProvider serviceProvider, IHubContext<TestHub> hubContext)
        {
            this.serviceProvider = serviceProvider;
            this.hubContext = hubContext;
        }

        public void Connect()
        {
            lock (botLock)
            {
                if (bot == null)
                {
                    bot = serviceProvider.GetService(typeof(IBot)) as IBot;
                    bot.MessageReceived = (message) => hubContext.Clients.All.SendAsync("ReceiveMessage", message).Wait();
                    bot.Connect();
                }
            }
        }

        public void SendMessage(string message)
        {
            bot.SendMessage(message);
        }
    }
}
