using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpikeCore.MessageBus;

namespace SpikeCore.Modules
{
    public class HelpModule : ModuleBase
    {
        public override string Name => "Help";
        public override string Description => "Provides help for the set of modules currently loaded.";
        public override string Instructions => "help [command]";

        // Prevent a circular dependency issue by lazily evaluating our modules.
        private readonly Lazy<IEnumerable<IModule>> _modules;

        public HelpModule(Lazy<IEnumerable<IModule>> modules)
        {
            _modules = modules;
        }

        protected override Task HandleMessageAsyncInternal(IrcChannelMessageMessage message, CancellationToken cancellationToken)
        {
            var splitMessage = message.Text.Split(" ");
            return splitMessage.Length <= 1 ? GetModules() : GetHelpForModule(splitMessage[1]);
        }

        private Task GetHelpForModule(string moduleName)
        {
            var module = _modules.Value.FirstOrDefault(x => x.Name.Equals(moduleName, StringComparison.InvariantCultureIgnoreCase));

            if (null != module)
            {
                var response = new List<string>
                {
                    "Module name: " + module.Name, 
                    "Module Description: " + module.Description,
                    "Module Instructions: " + module.Instructions
                };
                
                return SendMessagesToChannel(response);
            }

            return SendMessageToChannel("No such module exists, please try another.");
        }

        private Task GetModules()
        {
            return SendMessageToChannel("Modules list: " +
                                        string.Join(", ", _modules.Value.Select(module => module.Name).ToList()));
        }
    }
}