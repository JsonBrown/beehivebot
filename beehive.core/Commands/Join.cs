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
        private Match m;

        private readonly string me;
        private readonly string channel;
        private bool channelQueried;

        public Join(string me, string channel, ConcurrentDictionary<string, bool> users)
        {
            this.channel = channel;
            this.me = me;
            this.users = users;
        }

        public bool Parse(string command)
        {
            return !command.Contains("PRIVMSG") && (m = Regex.Match(command, ":(.*?)!(.*?) JOIN")).Success;
        }

        public List<IRCMessage> Execute()
        {
            var results = new List<IRCMessage>();
            var newUser = m.Groups[1].Value;
            if (newUser.ToLower() == me.ToLower())
            {
                if (!channelQueried)
                {
                    results.Add(new IRCMessage(Priority.Raw, String.Format("/NAMES {0}", channel)));
                    channelQueried = true;
                }
            } 
            else
            {
                users.TryAdd(newUser, false);
            }
            return results;            
        }
    }
}
