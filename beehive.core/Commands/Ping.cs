using beehive.common.Contracts;
using beehive.common.Enums;
using beehive.common.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace beehive.core.Commands
{
    public class Ping : ICommand
    {
        private Match m;

        public bool Parse(string command)
        {
            return !command.Contains("PRIVMSG") && (m = Regex.Match(command, @"PING.*? (.*?)$")).Success;
        }

        public List<CommandResult> Execute()
        {
            return new List<CommandResult>
            {
                new CommandResult(Priority.High, String.Format("PONG {0}", m.Groups[1].Value), "RawIrcResultProcessor")
            };
        }
    }
}
