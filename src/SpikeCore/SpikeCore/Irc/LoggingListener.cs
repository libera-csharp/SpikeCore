using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SpikeCore.MessageBus;

namespace SpikeCore.Irc
{
    public class LoggingListener : IMessageHandler<IrcPrivMessage>
    {
        public Task HandleMessageAsync(IrcPrivMessage message, CancellationToken cancellationToken)
        {
            Log.Information("{Type} {UserName} ({UserHostName}) -> {ChannelName}: {Text}", 
                "PRIVMSG", message.UserName, message.UserHostName, message.ChannelName ?? "<PM>", message.Text);
            
            return Task.CompletedTask;
        }
    }
}