﻿using beehive.common.Contracts;
using beehive.common.Enums;
using beehive.common.Objects;
using beehive.common.Extensions;
using beehive.core.Commands;
using beehive.core.Processors;
using beehive.extensions;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using beehive.extensions.Commands;
using beehive.extensions.Processors;

namespace beehive.core
{
    public class BeeHiveBot : IDisposable
    {
        private ILog log = LogManager.GetLogger(typeof(BeeHiveBot));

        private List<ICommand> commands;
        private Dictionary<string, IResultProcessor> ircProcessors;
        private Dictionary<string, IResultProcessor> generalProcessors;

        private Dictionary<QueueType, ConcurrentQueue<CommandResult>> queues;
        private ConcurrentDictionary<string, bool> users = new ConcurrentDictionary<string, bool>();

        private CancellationTokenSource cancel;
        private List<Task> handlers;

        private string nick, channel, password;
        private Irc irc;
        private readonly IDisk disk;
        public BeeHiveBot(string nick, string password, string channel, IDisk disk)
        {
            this.disk = disk;
            this.nick = nick;
            this.channel = String.Format("#{0}", channel);
            this.password = password;
        }

        public void Start()
        {
            this.irc = new Irc(nick, password, this.channel);
            this.queues = GetQueues();

            this.commands = new List<ICommand>
            {
                new Join(nick, this.channel, users),
                new Part(users),
                new Mode(users),
                new Ping()
            };
            // load from extensions lib
            GetCommands().ForEach(c => commands.Add(c));

            this.ircProcessors = new Dictionary<string, IResultProcessor>
            {
                {"RawIrcResultProcessor", new RawIrcResultProcessor(irc.Write)},
                {"IrcMessageResultProcessor", new IrcMessageResultProcessor(this.channel, irc.Write)}
            };
            // load from extensions lib
            this.generalProcessors = GetProcessors();
            StartThreads();
        }
        public void Stop()
        {
            cancel.Cancel();
        }

        private Dictionary<QueueType, ConcurrentQueue<CommandResult>> GetQueues()
        {
            return new Dictionary<QueueType, ConcurrentQueue<CommandResult>>
            {
                { QueueType.General, new ConcurrentQueue<CommandResult>() },
                { QueueType.IRC, new ConcurrentQueue<CommandResult>() }
            };
        }

        private Dictionary<string, IResultProcessor> GetProcessors()
        {
            return new Dictionary<string, IResultProcessor>
            {
                {"WCFWebResultsProcessor", new WCFWebResultsProcessor(disk)}
            };
        }

        private List<ICommand> GetCommands()
        {
            return new List<ICommand>
            {
                new CustomCommand(users),
                new Buzz(users),
                new QuoteCommand(users)
            };
        }

        private void StartThreads()
        {
            cancel = new CancellationTokenSource();
            var token = cancel.Token;
            handlers = new List<Task>()
            {
                new Task(() => Listen(token), token),
                new Task(() => HandleGeneralQueue(token), token),
                new Task(() => HandleIrcResponseCommandQueue(token), token)
            };
            handlers.ForEach(h => h.Start());
        }
        private void HandleIrcResponseCommandQueue(CancellationToken ct)
        {
            var queue = queues[QueueType.IRC];
            List<CommandResult> results = new List<CommandResult>();
            while (true)
            {
                CommandResult result;
                while (queue.TryDequeue(out result))
                {
                    results.Add(result);
                }
                if(results.Any())
                {
                    results.Combine()
                        .ForEach(r =>
                        {
                            try
                            {
                                ircProcessors[r.Processor].Process(result);
                            }
                            catch (Exception e)
                            {
                                log.Error(e);
                                throw;
                            }
                        });
                    results.Clear();
                }
                Thread.Sleep(5000);
                ct.ThrowIfCancellationRequested();
            }
        }
        private void HandleGeneralQueue(CancellationToken ct)
        {
            var queue = queues[QueueType.General];
            CommandResult result;
            while (true)
            {
                try
                {
                    while(queue.TryDequeue(out result))
                    {
                        generalProcessors[result.Processor].Process(result);
                    }
                } catch(Exception e)
                {
                    log.Error(e);
                    throw;
                }
                Thread.Sleep(500);
                ct.ThrowIfCancellationRequested();
            }
        }

        private void parseMessage(String message)
        {
            log.Debug(message);
            commands.ForEach(c => { if (c.Parse(message)) c.Execute().ForEach(m => queues[m.Queue].Enqueue(m)); });
        }
        private void Listen(CancellationToken ct)
        {
            try
            {
                while (irc.Connected)
                {
                    parseMessage(irc.Read.ReadLine());
                    ct.ThrowIfCancellationRequested();
                }
            }
            catch (IOException e){}
            catch (Exception e)
            {
                log.Error(e);
                throw;
            }
        }
        
        public void Dispose()
        {
            if (cancel != null ) cancel.Cancel();
            if (handlers != null) handlers.ForEach(h => h.ContinueWith((t) => t.Dispose()));
            if (irc != null) irc.Dispose();
            this.commands.ForEach(c => c.Dispose());
            this.generalProcessors.Values.ToList().ForEach(p => p.Dispose());
            this.ircProcessors.Values.ToList().ForEach(p => p.Dispose());
        }
    }
    public static class BeehiveBotExtensions
    {
        public static List<CommandResult> Combine(this IEnumerable<CommandResult> results)
        {
            return results.GroupBy(r => new { Queue = r.Queue, Processor = r.Processor, Message = r.Message })
                .Select(g => new CommandResult
                {
                    Processor = g.Key.Processor,
                    Queue = g.Key.Queue,
                    Message = String.Format("{0}{1}", g.All(r => r.User == null) ? String.Empty : String.Format("@ {0}\r\n", g.Select(c => c.User).Aggregate((p, n) => String.Format("{0},{1}", p, n))), g.Key.Message)
                })
                .ToList();
        }
    }
}
