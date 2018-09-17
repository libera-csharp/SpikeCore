using System.Collections.Generic;

namespace SpikeCore.Irc.Configuration
{
    public class BotConfig
    {
        public string Host { get; set; } 
        public int Port { get; set; }
        
        public string Nickname { get; set; }
        public List<string> Channels { get; set; }
    }
}