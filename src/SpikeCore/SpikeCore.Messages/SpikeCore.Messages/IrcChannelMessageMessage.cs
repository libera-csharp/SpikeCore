using Microsoft.AspNetCore.Identity;

namespace SpikeCore.Messages
{
    public class IrcChannelMessageMessage
    {
        public IdentityUser<string> IdentityUser { get; set; }
        public string ChannelName { get; set; }
        public string UserName { get; set; }
        public string UserHostName { get; set; }
        public string Text { get; set; }
    }
}