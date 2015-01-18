using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using log4net;
using System.Threading.Tasks;
using beehive.common.Contracts;
using beehive.common.Enums;
using beehive.core.Commands;
using beehive.common.Extensions;


namespace beehive.core
{
    public class Irc : IDisposable
    {
        private ILog log = LogManager.GetLogger(typeof(Irc));
        private String nick, password, channel,  admin;
        private TcpClient irc;
        private StreamReader read;
        private StreamWriter write;

        private List<ICommand> commands;
        private readonly Dictionary<Priority, ConcurrentQueue<string>> queues;
        private ConcurrentDictionary<string, bool> users = new ConcurrentDictionary<string, bool>();

        Thread listener;
        Thread KAthread;

        private Timer messageQueue;
        private int attempt;


        public Irc(string nick, string password, string channel)
        {

            this.nick = nick.ToLower();
            this.password = password;
            this.channel = channel.StartsWith("#") ? channel : String.Format("#{0}", channel);
            this.admin = !channel.StartsWith("#") ? channel : channel.Substring(1);
            this.commands = GetCommands();
            this.queues = GetQueues();

            Initialize();
        }


        private Dictionary<Priority, ConcurrentQueue<string>> GetQueues()
        {
            return new Dictionary<Priority, ConcurrentQueue<string>>
            {
                {Priority.Low, new ConcurrentQueue<string>()},
                {Priority.Medium, new ConcurrentQueue<string>()},
                {Priority.High, new ConcurrentQueue<string>()},
                {Priority.Raw, new ConcurrentQueue<string>()},
            };
        }

        private List<ICommand> GetCommands()
        {
            return new List<ICommand>
            {
                new Join(nick, channel, users),
                new Part(users),
                new Mode(users)
            };
        }

        private void Initialize()
        {

            Connect();
            StartThreads();
        }

        private void Connect()
        {
            if (irc != null)
            {
                irc.Close();
            }

            irc = new TcpClient();

            int count = 1;
            while (!irc.Connected)
            {
                log.DebugFormat("Connect attempt {0}", count);
                try
                {
                    irc.Connect("199.9.250.229", 443);

                    read = new StreamReader(irc.GetStream());
                    write = new StreamWriter(irc.GetStream());

                    write.AutoFlush = true;

                    sendRaw(String.Format("PASS {0} \r\n", password));
                    sendRaw(String.Format("NICK {0} \r\n", nick));
                    sendRaw(String.Format("USER {0} 8 * : {0}\r\n", nick));
                    sendRaw(String.Format("JOIN {0}\r\n", channel));
                }
                catch (SocketException e)
                {
                    log.DebugFormat("Unable to connect. Retrying in 5 seconds");
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Error Message (via Connect()): {0}", e);
                }
                count++;
                // Console.WriteLine("Connection failed.  Retrying in 5 seconds.");
                Thread.Sleep(5000);
            }            
        }

        private void StartThreads()
        {
            listener = new Thread(new ThreadStart(Listen));
            listener.Start();

            KAthread = new Thread(new ThreadStart(KeepAlive));
            KAthread.Start();

            messageQueue = new Timer(handleMessageQueue, null, 0, 4000);
        }

        private void Listen()
        {
            try
            {
                while (irc.Connected)
                {
                    parseMessage(read.ReadLine());
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

        private void KeepAlive()
        {
            while (true)
            {
                Thread.Sleep(30000);
                sendRaw("PING 1245");
            }
        }

        private void parseMessage(String message)
        {
            log.Debug(message);
            commands.ForEach(c => { if (c.Parse(message)) c.Execute().ForEach(m => sendMessage(m.Message, m.Queue)); } );
        }

        private void sendRaw(String message)
        {
            log.DebugFormat("Sending message: {0}", message);
            try
            {
                write.WriteLine(message);
                attempt = 0;
            }
            catch (Exception e)
            {
                attempt++;
                //Console.WriteLine("Can't send data.  Attempt: " + attempt);
                if (attempt >= 5)
                {
                    Console.WriteLine("Disconnected.  Attempting to reconnect.");
                    irc.Close();
                    //Flush();
                    Connect();
                    attempt = 0;
                }
            }
        }

        private void sendMessage(String message, Priority priority)
        {
            queues[priority].Enqueue(message);
        }
        private void handleMessageQueue(Object state)
        {
            String message;
            if (queues[Priority.Raw].TryDequeue(out message))
            {
                sendRaw(message);
            }
            else if (queues[Priority.High].TryDequeue(out message))
            {
                sendRaw("PRIVMSG " + channel + " :" + message);
                messageQueue.Change(4000, Timeout.Infinite);
            }
            else if (queues[Priority.Medium].TryDequeue(out message))
            {
                sendRaw("PRIVMSG " + channel + " :" + message);
                messageQueue.Change(4000, Timeout.Infinite);
            }
            else if (queues[Priority.Low].TryDequeue(out message))
            {
                sendRaw("PRIVMSG " + channel + " :" + message);
                messageQueue.Change(4000, Timeout.Infinite);
            }
            else messageQueue.Change(4000, Timeout.Infinite);
        }

        public void Dispose()
        {
            if (listener != null) listener.Abort();
            if (KAthread != null) KAthread.Abort();
        }
    }
}
