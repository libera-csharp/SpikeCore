using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SpikeCore.Data;
using SpikeCore.Data.Models;
using SpikeCore.MessageBus;

namespace SpikeCore.Modules
{
    public class FactoidModule : ModuleBase
    {
        public override string Name => "Factoids";
        public override string Description => "Keeps track of factoids.";
        public override string Instructions => "<ban|idiot|warn> <message>";
        public override IEnumerable<string> Triggers => new List<string> { "ban", "idiot", "warn" };

        private readonly SpikeCoreDbContext _context;
        private const int FactDisplayCount = 5;
        private static readonly Regex FactoidRegex = new Regex(@"~(idiot|ban|warn)\s(\S+)\s?(.*)?");

        public FactoidModule(SpikeCoreDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleMessageAsyncInternal(IrcPrivMessage request, CancellationToken cancellationToken)
        {
            var match = FactoidRegex.Match(request.Text);

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
                        CreatedBy = request.UserName,
                        Name = name
                    };

                    await SaveOrUpdate(factoid, cancellationToken);
                    await SendResponse(request, "Factoid saved.");
                }

                // Otherwise we're looking up factoids by name/type.
                else
                {
                    var query = _context.Factoids
                        .Where(factoid => factoid.Type.ToLower() == command.ToLower() && factoid.Name.ToLower() == name.ToLower());

                    var matchingFactoids = query
                        .OrderByDescending(factoid => factoid.CreationDate)
                        .Take(FactDisplayCount)
                        .ToList();

                    int count = matchingFactoids.Count;
                    if (count > 0)
                    {
                        // If we retrieved the limit, check so we can tell users if there's more facts they can't see.
                        // They'll have to hit the web UI for these.
                        if (count == FactDisplayCount)
                        {
                            count = query.Count();
                        }

                        var pluralization = (count == 1) ? string.Empty : "s";

                        var paginationDisplay = (count > FactDisplayCount)
                            ? $" (showing the first {FactDisplayCount})"
                            : string.Empty;

                        var factoidDisplay = matchingFactoids
                            .Select(x => $"[{x.Description} at {x.CreationDate:MM/dd/yyyy H:mm:ss UTC} by {x.CreatedBy}]");

                        var response =
                            $"Found {count} factoid{pluralization} of type {command}{paginationDisplay} for {name}: {String.Join(", ", factoidDisplay)}";

                        await SendResponse(request, response);
                    }
                    else
                    {
                        await SendResponse(request, $"No factoids for {name} of type {command}");
                    }
                }
            }
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