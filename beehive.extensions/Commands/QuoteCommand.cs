using beehive.common.Contracts;
using beehive.common.Objects;
using beehive.data;
using beehive.data.model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using beehive.common.Extensions;
using beehive.common.Enums;

namespace beehive.extensions.Commands
{
    public class QuoteCommand : ICommand
    {
        private Match m;
        private readonly ConcurrentDictionary<string, bool> users;
        private readonly IContext data;
        private readonly List<Quote> quotes;
        private readonly Dictionary<string, Action<Queue<string>, string>> adminCommands;


        public QuoteCommand(ConcurrentDictionary<string, bool> users):
        this(new BeehiveContext(ConfigurationManager.ConnectionStrings["BeeHive"].ConnectionString), users) {}

        public QuoteCommand(IContext data, ConcurrentDictionary<string, bool> users)
        {
            this.data = data;
            this.users = users;
            this.quotes = data.Get<Quote>().ToList();
            this.adminCommands = GetAdmin();
        }
        public bool Parse(string command)
        {
            return command.Contains("PRIVMSG") && (m = Regex.Match(command, @":(.*?)!.*?PRIVMSG.*?:\s*((!addquote|!delquote|!quote).*)$")).Success;
        }

        public List<CommandResult> Execute()
        {
            var results = new List<CommandResult>();
            var user = m.Groups[1].Value;
            var q = new Queue<string>(m.Groups[2].Value.Split(' '));

            var command = q.Dequeue().ToLower();

            if (adminCommands.ContainsKey(command) && users.Get(user.ToLower())) adminCommands[command](q, user);
            else
            {
                if (command == "!quote")
                {
                    var id = -1;
                    if (!int.TryParse(q.Dequeue(), out id)) id = (new Random()).Next(0, quotes.Count);
                    Quote quote = this.quotes.Where(qu => qu.Id == id).FirstOrDefault();
                    if (quote != null)
                    {
                        results.Add(new CommandResult(QueueType.IRC, quote.Text, "IrcMessageResultProcessor", user));
                    } 
                }
            }
            return results;
        }

        private Dictionary<string, Action<Queue<string>, string>> GetAdmin()
        {
            return new Dictionary<string, Action<Queue<string>, string>>
            {
                {"!addquote", (q,u) => {
                    var response = String.Join(" ", q.ToList());
                    var quote = new Quote
                    {
                        Text = String.Join(" ", q.ToList()),
                        AddedBy = u
                    };
                    data.Add(quote);
                    data.Save();
                    this.quotes.Add(quote);
                }},
                {"!delquote", (q,u) => {
                    var id = 0;
                    Quote quote = null;
                    if (int.TryParse(q.Dequeue(), out id) && (quote = this.quotes.Where(qu => qu.Id == id).FirstOrDefault()) != null)
                    {
                        data.Delete(quote);
                        this.quotes.Remove(quote);
                        data.Save();
                    }
                }}
            };
        }

        public void Dispose()
        {
            if (this.data != null) this.data.Dispose();
        }
    }
}
