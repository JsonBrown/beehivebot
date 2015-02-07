using beehive.common.Contracts;
using beehive.common.Enums;
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
    public class Part : ICommand
    {
        private ConcurrentDictionary<string, bool> users;
        private Match m;

        public Part(ConcurrentDictionary<string, bool> users)
        {
            this.users = users;
        }

        public bool Parse(string command)
        {
            return !command.Contains("PRVTMSG") && (m = Regex.Match(command, ":(.*?)!(.*?) PART")).Success;
        }

        public List<CommandResult> Execute()
        {
            var leavingUser = m.Groups[1].Value;
            var mod = false;
            users.TryRemove(leavingUser, out mod);
            return new List<CommandResult>();
        }

        public void Dispose() { }
    }
}
