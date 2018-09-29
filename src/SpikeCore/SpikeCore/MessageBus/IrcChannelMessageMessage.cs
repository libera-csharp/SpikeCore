using SpikeCore.Data.Models;

namespace SpikeCore.MessageBus
{
    public class IrcChannelMessageMessage
    {
        public SpikeCoreUser IdentityUser { get; set; }
        public string ChannelName { get; set; }
        public string UserName { get; set; }
        public string UserHostName { get; set; }
        public string Text { get; set; }
    }
}