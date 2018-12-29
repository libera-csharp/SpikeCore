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

        protected override Task HandleMessageAsyncInternal(IrcPrivMessage request, CancellationToken cancellationToken)
        {
            var splitMessage = request.Text.Split(" ");
            return splitMessage.Length <= 1 ? GetModules(request) : GetHelpForModule(request, splitMessage[1]);
        }

        private Task GetHelpForModule(IrcPrivMessage request, string moduleName)
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
                
                return SendMessagesToNick(request.UserName, response);
            }

            return SendMessageToNick(request.UserName, "No such module exists, please try another.");
        }

        private Task GetModules(IrcPrivMessage request)
        {
            return SendMessageToNick(request.UserName, "Modules list: " + string.Join(", ", _modules.Value.Select(module => module.Name).ToList()));
        }
    }
}