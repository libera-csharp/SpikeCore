using System.Threading;
using System.Threading.Tasks;

namespace SpikeCore.Messages
{
    public interface IMessageHandler<TMessage>
    {
        Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken);
    }
}
