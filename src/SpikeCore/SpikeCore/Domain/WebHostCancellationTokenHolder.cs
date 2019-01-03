using System.Threading;

namespace SpikeCore.Domain
{
    public class WebHostCancellationTokenHolder
    {
        public CancellationTokenSource CancellationTokenSource { get; private set; }
        
        public WebHostCancellationTokenHolder(CancellationTokenSource cancellationTokenSource)
        {
            CancellationTokenSource = cancellationTokenSource;
        } 
    }
}