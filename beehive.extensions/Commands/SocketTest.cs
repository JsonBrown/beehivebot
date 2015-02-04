using beehive.common.Contracts;
using beehive.common.Enums;
using beehive.common.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace beehive.extensions.Commands
{
    public class SocketTest : ICommand
    {
        private string command;
        public bool Parse(string command)
        {
            this.command = command;
            return command.Contains("PRIVMSG");
        }

        public List<CommandResult> Execute()
        {
            return new List<CommandResult>()
            {
                new CommandResult(QueueType.General, command, "WCFWebResultsProcessor")
            };
        }

        public void Dispose()
        {
            
        }
    }
}
