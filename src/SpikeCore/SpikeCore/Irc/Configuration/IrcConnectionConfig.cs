using System.Collections.Generic;

namespace SpikeCore.Irc.Configuration
{
    public class IrcConnectionConfig
    {
        public string Host { get; set; } 
        public int Port { get; set; }
        
        public string Nickname { get; set; }
        public List<string> Channels { get; set; }
    }
}