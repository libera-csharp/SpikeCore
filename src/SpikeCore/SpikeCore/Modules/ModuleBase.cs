using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using SpikeCore.Data.Models;
using SpikeCore.Irc.Configuration;
using SpikeCore.MessageBus;

namespace SpikeCore.Modules
{
    public abstract class ModuleBase : IMessageHandler<IrcChannelMessageMessage>, IModule
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Instructions { get; }

        public virtual IEnumerable<string> Triggers => new List<string> {Name};

        public IMessageBus MessageBus { private get; set; }
        public ModuleConfiguration Configuration { private get; set; }

        // TODO [Kog 10/06/2018] : work in access checks etc.
        public Task HandleMessageAsync(IrcChannelMessageMessage message, CancellationToken cancellationToken)
        {
            return Triggers.Any(trigger =>
                message.Text.StartsWith(Configuration.TriggerPrefix + trigger,
                    StringComparison.InvariantCultureIgnoreCase) && AccessAllowed(message.IdentityUser))
                ? HandleMessageAsyncInternal(message, cancellationToken)
                : Task.CompletedTask;
        }


        protected abstract Task HandleMessageAsyncInternal(IrcChannelMessageMessage message,
            CancellationToken cancellationToken);

        protected Task SendMessageToChannel(string channelName, string message)
            => MessageBus.PublishAsync(new IrcSendChannelMessage(channelName, message));

        protected Task SendMessagesToChannel(string channelName, IEnumerable<string> messages)
            => MessageBus.PublishAsync(new IrcSendChannelMessage(channelName, messages));

        protected virtual bool AccessAllowed(SpikeCoreUser user)
        {
            // By default a command can be run by any known user with at least one role.
            return null != user && user.Roles.Any();
        }
    }
}