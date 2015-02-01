using beehive.common.Contracts;
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
using beehive.data;

namespace beehive.core
{
    public class BeeHiveBot : IDisposable
    {
        private ILog log = LogManager.GetLogger(typeof(BeeHiveBot));

        private List<ICommand> commands;
        private Dictionary<string, IResultProcessor> ircProcessors;
        private Dictionary<string, IResultProcessor> generalProcessors;

        private readonly Dictionary<QueueType, ConcurrentQueue<CommandResult>> queues;
        private ConcurrentDictionary<string, bool> users = new ConcurrentDictionary<string, bool>();

        Thread listener;
        Thread generalQueueHandler;
        private Timer ircResponseQueueHandler;

        private string nick, channel;
        private Irc irc;
        private readonly IDisk disk;
        private readonly IContext data;
        public BeeHiveBot(string nick, string password, string channel, IDisk disk, IContext data)
        {
            this.data = data;
            this.disk = disk;
            this.nick = nick;
            this.channel = String.Format("#{0}", channel);
            this.irc = new Irc(nick, password, channel);
            this.commands = GetCommands();
            this.queues = GetQueues();
            this.generalProcessors = GetProcessors().Select((i, g) =>
            {
                this.ircProcessors = i;
                return g;
            });

            StartThreads();
        }

        private Dictionary<QueueType, ConcurrentQueue<CommandResult>> GetQueues()
        {
            return new Dictionary<QueueType, ConcurrentQueue<CommandResult>>
            {
                { QueueType.General, new ConcurrentQueue<CommandResult>() },
                { QueueType.IRC, new ConcurrentQueue<CommandResult>() }
            };
        }

        private Tuple<Dictionary<string, IResultProcessor>, Dictionary<string, IResultProcessor>> GetProcessors()
        {
            return new Tuple<Dictionary<string, IResultProcessor>, Dictionary<string, IResultProcessor>>
            (new Dictionary<string, IResultProcessor>
            {
                {"RawIrcResultProcessor", new RawIrcResultProcessor(irc.Write)},
                {"IrcMessageResultProcessor", new IrcMessageResultProcessor(channel, irc.Write)}
            },
            new Dictionary<string, IResultProcessor>
            {
                // this will be loaded through MEF
                {"WCFWebResultsProcessor", new WCFWebResultsProcessor(disk)}
            });
        }

        private List<ICommand> GetCommands()
        {
            return new List<ICommand>
            {
                new Join(nick, channel, users),
                new Part(users),
                new Mode(users),
                new Ping(),
                new CustomCommand(data, users)
            };
        }

        private void StartThreads()
        {
            listener = new Thread(new ThreadStart(Listen));
            listener.Start();

            generalQueueHandler = new Thread(new ThreadStart(HandleGeneralQueue));
            generalQueueHandler.Start();

            ircResponseQueueHandler = new Timer(HandleIrcResponseCommandQueue, null, 0, 4000);
        }
        private void HandleIrcResponseCommandQueue(Object state)
        {
            var queue = queues[QueueType.IRC];
            CommandResult result;
            if (queue.TryDequeue(out result))
            {
                ircProcessors[result.Processor].Process(result);
            }
            ircResponseQueueHandler.Change(4000, Timeout.Infinite);
        }
        private void HandleGeneralQueue()
        {
            var queue = queues[QueueType.General];
            CommandResult result;
            while (true)
            {
                if (queue.TryDequeue(out result))
                {
                    generalProcessors[result.Processor].Process(result);
                }
            }
        }
        private void parseMessage(String message)
        {
            log.Debug(message);
            commands.ForEach(c => { if (c.Parse(message)) c.Execute().ForEach(m => queues[m.Queue].Enqueue(m)); });
        }
        private void Listen()
        {
            try
            {
                while (irc.Connected)
                {
                    parseMessage(irc.Read.ReadLine());
                }
            }
            catch (IOException e)
            {

            }
            catch (Exception e)
            {
                log.ErrorFormat("Error Message (via Listen()): {0}", e);
            }
        }
        
        public void Dispose()
        {
            if (listener != null) listener.Abort();
            if (generalQueueHandler != null) generalQueueHandler.Abort();
            if (ircResponseQueueHandler != null) ircResponseQueueHandler.Dispose();
            if (irc != null) irc.Dispose();
        }
    }
}
