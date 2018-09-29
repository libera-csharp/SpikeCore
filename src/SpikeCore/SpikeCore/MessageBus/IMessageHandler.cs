using System.Threading;
using System.Threading.Tasks;

namespace SpikeCore.MessageBus
{
    public interface IMessageHandler<TMessage>
    {
        Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken);
    }
}
