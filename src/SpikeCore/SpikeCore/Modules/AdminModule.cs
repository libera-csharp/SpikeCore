using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
            return base.AccessAllowed(user) && user.Roles.Any(x => x.Equals("Admin", StringComparison.InvariantCultureIgnoreCase));
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
                    IrcClient.Quit(details);
                }
                
                if (command.Equals("join", StringComparison.InvariantCultureIgnoreCase))
                {
                    IrcClient.JoinChannel(details);
                }

                if (command.Equals("part", StringComparison.InvariantCultureIgnoreCase))
                {
                    var partMatch = PartRegex.Match(details);
                    if (partMatch.Success)
                    {
                        IrcClient.PartChannel(partMatch.Groups[1].Value, partMatch.Groups[2].Value);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}