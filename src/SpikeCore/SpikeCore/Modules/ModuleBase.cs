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
    public abstract class ModuleBase : IMessageHandler<IrcPrivMessage>, IModule
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Instructions { get; }

        public virtual IEnumerable<string> Triggers => new List<string> {Name};

        public IMessageBus MessageBus { protected get; set; }
        public ModuleConfiguration Configuration { protected get; set; }
        
        public Task HandleMessageAsync(IrcPrivMessage message, CancellationToken cancellationToken)
        {
            return Triggers.Any(trigger =>
                message.Text.StartsWith(Configuration.TriggerPrefix + trigger, StringComparison.InvariantCultureIgnoreCase) && AccessAllowed(message.IdentityUser))
                ? HandleMessageAsyncInternal(message, cancellationToken)
                : Task.CompletedTask;
        }

        protected abstract Task HandleMessageAsyncInternal(IrcPrivMessage request,
            CancellationToken cancellationToken);

        protected Task SendResponse(IrcPrivMessage request, string message)
        {
            return request.Private ? SendMessageToNick(request.UserName, message) : SendMessageToChannel(request.ChannelName, message);
        }

        protected Task SendResponse(IrcPrivMessage request, IEnumerable<string> messages)
        {
            return request.Private ? SendMessagesToNick(request.UserName, messages) : SendMessagesToChannel(request.ChannelName, messages);
        }
        
        protected Task SendMessageToChannel(string channelName, string message)
            => MessageBus.PublishAsync(new IrcSendChannelMessage(channelName, message));

        protected Task SendMessagesToChannel(string channelName, IEnumerable<string> messages)
            => MessageBus.PublishAsync(new IrcSendChannelMessage(channelName, messages));

        protected Task SendMessageToNick(string nick, string message)
            => MessageBus.PublishAsync(new IrcSendPrivateMessage(nick, message));
        
        protected Task SendMessagesToNick(string nick, IEnumerable<string> messages)
            => MessageBus.PublishAsync(new IrcSendPrivateMessage(nick, messages));
        
        protected virtual bool AccessAllowed(SpikeCoreUser user)
        {
            // By default a command can be run by any known user with at least one role.
            return null != user && user.Roles.Any();
        }
    }
}