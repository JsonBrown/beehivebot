using beehive.common.Contracts;
using beehive.common.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace beehive.core.Commands
{
    public class Mode : ICommand
    {
        private ConcurrentDictionary<string, bool> users;
        private Match m;

        public Mode(ConcurrentDictionary<string, bool> users)
        {
            this.users = users;
        }
        public bool Parse(string command)
        {
            return !command.Contains("PRIVMSG") && (m = Regex.Match(command, @":.*?MODE.*?([\+-])o (.*?)$")).Success;
        }

        public List<CommandResult> Execute()
        {
            var user = m.Groups[2].Value;
            var op = m.Groups[1].Value == "+";
            
            users.AddOrUpdate(user, op, (u,o) => op);
            return new List<CommandResult>();
        }

        public void Dispose() { }
    }
}
