using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SpikeCore.Data;
using SpikeCore.Data.Models;
using SpikeCore.MessageBus;

namespace SpikeCore.Modules
{
    public class KarmaModule : ModuleBase
    {
        public override string Name => "Karma";
        public override string Description => "The wheel in the sky keeps on turning";
        public override string Instructions => "karma <user>(++|--)" ;

        private readonly SpikeCoreDbContext _context;       
        private static readonly Regex KarmaRegex = new Regex(@"~karma\s([^\-\+]+)((\-\-)|(\+\+))?");
        
        public KarmaModule(SpikeCoreDbContext context)
        {
            _context = context;
        }
        
        protected override async Task HandleMessageAsyncInternal(IrcChannelMessageMessage message, CancellationToken cancellationToken)
        {
            var match = KarmaRegex.Match(message.Text);

            if (match.Success)
            {
                var nick = match.Groups[1].Value;
                var op = match.Groups[2].Value;
                
                var user = _context.Karma
                               .SingleOrDefault(k => k.Name.Equals(nick, StringComparison.CurrentCultureIgnoreCase)) ?? new KarmaItem { Name = nick, Karma = 0 };

                // Ignore anyone tweaking their own karma.
                if (string.IsNullOrEmpty(op) ||!nick.Equals(message.UserName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (op.Equals("--"))
                    {
                        user.Karma--;
                    } 
                    else if (op.Equals("++"))
                    {
                        user.Karma++;
                    }
                    
                    await SaveOrUpdate(user, cancellationToken);            
                    await SendMessageToChannel(message.ChannelName, $"{user.Name} has a karma of {user.Karma}");   
                }                  
            }
        }

        // TODO [Kog 10/06/2018] : Looks like we're gonna need some repo infrastructure.     
        private async Task SaveOrUpdate(KarmaItem karmaItem, CancellationToken cancellationToken)
        {
            if (karmaItem.Id == 0)
            {
                await _context.Karma.AddAsync(karmaItem, cancellationToken);    
            }
            else
            {
                _context.Karma.Update(karmaItem);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}