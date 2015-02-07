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
    public class Join : ICommand
    {
        private ConcurrentDictionary<string, bool> users;
        private Match m1;
        private Match m2;

        private readonly string me;
        private readonly string channel;

        public Join(string me, string channel, ConcurrentDictionary<string, bool> users)
        {
            this.channel = channel;
            this.me = me;
            this.users = users;
        }

        public bool Parse(string command)
        {
            return !command.Contains("PRIVMSG") &&
                ((m1 = Regex.Match(command, ":(.*?)!(.*?) JOIN")).Success || (m2 = Regex.Match(command, "353.*?:(.*?)$")).Success);
        }
        public List<CommandResult> Execute()
        {
            return (m1.Success) ? ParseJoin() : ParseChannelQuery();
        }

        private List<CommandResult> ParseChannelQuery()
        {
            m2.Groups[1].Value.Split(' ').ToList()
                .ForEach(u => 
                {
                    if (u.ToLower() != me.ToLower()) users.TryAdd(u, false);
                });

            return new List<CommandResult>();
        }

        private List<CommandResult> ParseJoin()
        {
            var newUser = m1.Groups[1].Value;
            if (newUser.ToLower() != me.ToLower()) users.TryAdd(newUser, false);

            return new List<CommandResult>();            
        }

        public void Dispose() { }
    }
}
