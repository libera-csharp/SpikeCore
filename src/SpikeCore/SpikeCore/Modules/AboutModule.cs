using System;
using System.Threading;
using System.Threading.Tasks;

using SpikeCore.MessageBus;

namespace SpikeCore.Modules
{
    public class AboutModule : ModuleBase
    {
        public override string Name => "About";
        public override string Description => "Provides information about the bot.";
        public override string Instructions => "about" ;

        private string[] _taglines => new[] {"now in stereo!", "with Smell-O-Vision!", "filmed in technicolor!"};
        private readonly Random _random = new Random();

        protected override async Task HandleMessageAsyncInternal(IrcChannelMessageMessage message, CancellationToken cancellationToken)
        {
            await SendMessageToChannel(message.ChannelName, "SpikeCore: " + _taglines[_random.Next(0, _taglines.Length)]);
        }
    }
}