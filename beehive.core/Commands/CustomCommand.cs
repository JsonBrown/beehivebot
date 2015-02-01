using beehive.common.Contracts;
using beehive.common.Objects;
using beehive.data;
using db = beehive.data.model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using beehive.common.Enums;

namespace beehive.core.Commands
{
    public class CustomCommand : ICommand
    {
        private Match m;

        private readonly ConcurrentDictionary<string, bool> users;
        private readonly IContext data;
        private readonly Dictionary<string, db.CustomCommand> responses;
        private readonly Dictionary<string, Action<Queue<string>>> adminCommands;
        public CustomCommand(IContext data, ConcurrentDictionary<string, bool> users)
        {
            this.data = data;
            this.users = users;
            this.responses = data.Get<db.CustomCommand>().ToDictionary(cc => cc.Command, cc => cc);
            this.adminCommands = GetAdmin();
        }
        public bool Parse(string command)
        {
            return command.Contains("PRIVMSG") && (m = Regex.Match(command, ":(.*?)!.*?PRIVMSG.*?:(!.*?)$")).Success;
        }

        public List<CommandResult> Execute()
        {
            var results = new List<CommandResult>();
            var user = m.Groups[1].Value;
            var q = new Queue<string>(m.Groups[2].Value.Split(' '));

            var command = q.Dequeue().ToLower();

            if (adminCommands.ContainsKey(command) && users[user.ToLower()]) adminCommands[command](q);
            else
            {
                if (responses.ContainsKey(command))
                {
                    results.Add(new CommandResult(QueueType.IRC, responses[command].Response, "IrcMessageResultProcessor"));
                }
            }
            return results;
        }

        private Dictionary<string, Action<Queue<string>>> GetAdmin()
        {
            return new Dictionary<string, Action<Queue<string>>>
            {
                {"!addcmd", (q) => {
                    var newCommand = q.Dequeue();
                    newCommand = newCommand.StartsWith("!") ? newCommand : String.Format("!{0}", newCommand);
                    var response = String.Join(" ", q.ToList());
                    var custom = new db.CustomCommand
                    {
                        Command = newCommand,
                        Response = response
                    };
                    data.Add(custom);
                    data.Save();
                    this.responses.Add(newCommand, custom);
                }},
                {"!deletecmd", (q) => {
                    var newCommand = q.Dequeue();
                    if (this.responses.ContainsKey(newCommand))
                    {
                        data.Delete(this.responses[newCommand]);
                        this.responses.Remove(newCommand);
                    }
                }}
            };
        }
    }
}
