using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Foundatio.Messaging;

using SpikeCore.Irc.Configuration;
using SpikeCore.MessageBus;

namespace SpikeCore.Modules
{
    public abstract class ModuleBase : IMessageHandler<IrcChannelMessageMessage>, IModule
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Instructions { get; }

        public IMessageBus MessageBus { private get; set; }
        public ModuleConfiguration Configuration { get; set; }

        // TODO [Kog 10/06/2018] : work in access checks etc.
        public Task HandleMessageAsync(IrcChannelMessageMessage message, CancellationToken cancellationToken)
            => message.Text.StartsWith(Configuration.TriggerPrefix + Name, StringComparison.InvariantCultureIgnoreCase) && message.IdentityUser != null
                ? HandleMessageAsyncInternal(message, cancellationToken)
                : Task.CompletedTask;

        protected abstract Task HandleMessageAsyncInternal(IrcChannelMessageMessage message, CancellationToken cancellationToken);

        protected Task SendMessageToChannel(string channelName, string message)
            => MessageBus.PublishAsync(new IrcSendChannelMessage(channelName, message));

        protected Task SendMessagesToChannel(string channelName, IEnumerable<string> messages)
            => MessageBus.PublishAsync(new IrcSendChannelMessage(channelName, messages));
    }
}