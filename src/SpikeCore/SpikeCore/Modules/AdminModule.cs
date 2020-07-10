using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using SpikeCore.Data.Models;
using SpikeCore.MessageBus;

namespace SpikeCore.Modules
{
    public class AdminModule : ModuleBase
    {
        private static readonly Regex Regex = new Regex(@"~(quit|join|part)\s(.*)?");
        private static readonly Regex PartRegex = new Regex(@"(\S+)\s(.*)?");
        
        public override string Name => "Admin";
        public override string Description => "Provides administrative features, such as quitting networks.";
        public override string Instructions => "quit <message>|join <channel>|part <channel> <reason>" ;
        public override IEnumerable<string> Triggers => new List<string> { "quit", "join", "part"};

        protected override bool AccessAllowed(SpikeCoreUser user)
        {
            return base.AccessAllowed(user) && user.isAdmin();
        }

        protected override Task HandleMessageAsyncInternal(IrcPrivMessage request, CancellationToken cancellationToken)
        {
            var match = Regex.Match(request.Text);
            if (match.Success)
            {
                var command = match.Groups[1].Value;
                var details = match.Groups[2].Value;

                if (command.Equals("quit", StringComparison.InvariantCultureIgnoreCase))
                {
                    return MessageBus.PublishAsync(new IrcQuitMessage(details));
                }
                
                if (command.Equals("join", StringComparison.InvariantCultureIgnoreCase))
                {
                    return MessageBus.PublishAsync(new IrcJoinChannelMessage(details));
                }

                if (command.Equals("part", StringComparison.InvariantCultureIgnoreCase))
                {
                    var partMatch = PartRegex.Match(details);
                    if (partMatch.Success)
                    {
                        return MessageBus.PublishAsync(new IrcPartChannelMessage(partMatch.Groups[1].Value,partMatch.Groups[2].Value));
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}