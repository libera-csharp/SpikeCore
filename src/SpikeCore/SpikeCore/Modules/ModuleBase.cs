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

        public Task HandleMessageAsync(IrcChannelMessageMessage message, CancellationToken cancellationToken)
        {
            // TODO [Kog 10/06/2018] : work in access checks etc.
            return message.Text.StartsWith(Configuration.TriggerPrefix + Name, StringComparison.InvariantCultureIgnoreCase)
                ? HandleMessageAsyncInternal(message, cancellationToken)
                : Task.CompletedTask;
        }

        protected abstract Task HandleMessageAsyncInternal(IrcChannelMessageMessage message, CancellationToken cancellationToken);

        // TODO [Kog 10/06/2018] : Need to wire this back to the originating channel. We don't have a way to do that right now.
        protected Task SendMessageToChannel(string message)
        {
            return MessageBus.PublishAsync(new IrcSendMessage(message));
        }

        protected Task SendMessagesToChannel(IEnumerable<string> messages)
        {
            return MessageBus.PublishAsync(new IrcSendMessage(messages));
        }
    }
}