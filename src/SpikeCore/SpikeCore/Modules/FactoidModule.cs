using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using SpikeCore.Data;
using SpikeCore.Data.Models;
using SpikeCore.MessageBus;

namespace SpikeCore.Modules
{
    public class FactoidModule : ModuleBase
    {
        public override string Name => "factoids";
        public override string Description => "Keeps track of factoids.";
        public override string Instructions => "factoid <ban|idiot|warn> <message>";

        private readonly SpikeCoreDbContext _context;
        private const int FactDisplayCount = 5;
        private static readonly Regex FactoidRegex = new Regex(@"~factoids\s(\w+)\s(\S+)\s?(.*)?");

        public FactoidModule(SpikeCoreDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleMessageAsyncInternal(IrcChannelMessageMessage message, CancellationToken cancellationToken)
        {
            var match = FactoidRegex.Match(message.Text);

            if (match.Success)
            {
                var command = match.Groups[1].Value;
                var name = match.Groups[2].Value;
                var description = match.Groups[3].Value;

                // We're adding a new factoid.
                if (description.Length > 0)
                {
                    var factoid = new Factoid
                    {
                        Description = description,
                        Type = command,
                        CreationDate = DateTime.UtcNow,
                        CreatedBy = message.UserName,
                        Name = name
                    };

                    await SaveOrUpdate(factoid, cancellationToken);
                    await SendMessageToChannel(message.ChannelName, "Factoid saved.");
                }

                // Otherwise we're looking up factoids by name/type.
                else
                {
                    // Grab the total count so we can tell users if there's more facts they can't see. They'll have to
                    // hit the web UI for these.
                    var count = _context.Factoids
                        .Count(factoid => FactoidMatches(factoid, command, name));                   

                    if (count > 0)
                    {
                        var matchingFactoids = _context.Factoids
                            .Where(factoid => FactoidMatches(factoid, command, name))
                            .OrderByDescending(factoid => factoid.CreationDate)
                            .ToList();
                        
                        var pluralization = (count == 1) ? string.Empty : "s";

                        var paginationDisplay = (count > FactDisplayCount)
                            ? string.Format(" (showing the first {0})", Math.Min(count, FactDisplayCount))
                            : string.Empty;
                        
                        var factoidDisplay = matchingFactoids
                            .Take(FactDisplayCount)
                            .Select(x => string.Format("[{0} at {1} by {2}]", x.Description,
                                x.CreationDate.ToString("MM/dd/yyyy H:mm:ss UTC"), x.CreatedBy))
                            .Join();

                        var response =
                            $"Found {count} factoid{pluralization} of type {command}{paginationDisplay} for {name}: {factoidDisplay}";

                        await SendMessageToChannel(message.ChannelName, response);
                    }
                    else
                    {
                        await SendMessageToChannel(message.ChannelName,
                            $"No factoids for {name} of type {command}");
                    }
                }
            }
        }

        private static bool FactoidMatches(Factoid factoid, string command, string name)
        {
            return factoid.Type.Equals(command, StringComparison.InvariantCultureIgnoreCase) &&
                   factoid.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }

        private async Task SaveOrUpdate(Factoid factoid, CancellationToken cancellationToken)
        {
            if (factoid.Id == 0)
            {
                await _context.Factoids.AddAsync(factoid, cancellationToken);
            }
            else
            {
                _context.Factoids.Update(factoid);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}