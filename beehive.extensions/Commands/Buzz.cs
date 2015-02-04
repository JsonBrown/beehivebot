using beehive.common.Contracts;
using beehive.common.Enums;
using beehive.common.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace beehive.extensions.Commands
{
    public class Buzz : ICommand
    {
        private Match m;
        private readonly ConcurrentDictionary<string, bool> users;
        public Buzz(ConcurrentDictionary<string, bool> users)
        {
            this.users = users;
        }
        public bool Parse(string command)
        {
            return command.Contains("PRIVMSG") && (m = Regex.Match(command, @":(.*?)!.*?PRIVMSG.*?:\s*(!buzz.*?)$",RegexOptions.IgnoreCase)).Success;
        }

        public List<CommandResult> Execute()
        {
            var user = m.Groups[1].Value;
            return new List<CommandResult>
            {
                new CommandResult(QueueType.General, JsonConvert.SerializeObject(new { User = user, Date = DateTime.Now, Type = "buzz" }), "WCFWebResultsProcessor")
            };
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
