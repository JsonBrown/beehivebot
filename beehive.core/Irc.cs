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

namespace beehive.core
{
    public class Irc : IDisposable
    {
        private ILog log = LogManager.GetLogger(typeof(Irc));
        private String nick, password, channel,  admin, user = "";
        private int[] intervals = { 1, 2, 3, 4, 5, 6, 10, 12, 15, 20, 30, 60 };
        private TcpClient irc;
        private StreamReader read;
        private StreamWriter write;
        private List<string> users = new List<string>();
        private List<string> usersToLookup = new List<string>();
        private ConcurrentQueue<string> highPriority = new ConcurrentQueue<string>();
        private ConcurrentQueue<string> normalPriority = new ConcurrentQueue<string>();
        private ConcurrentQueue<string> lowPriority = new ConcurrentQueue<string>();
        private Thread listener;
        private Thread timerThread;
        private Thread KAthread;
        private Timer messageQueue;
        private int attempt;


        public Irc(String nick, String password, String channel)
        {
            setNick(nick);
            setPassword(password);
            setChannel(channel);
            setAdmin(channel);

            Initialize();
        }

        private void Initialize()
        {
            Connect();
        }

        private void Connect()
        {
            if (irc != null)
            {
                //Console.WriteLine("Irc connection already exists.  Closing it and opening a new one.");
                irc.Close();
            }

            irc = new TcpClient();

            int count = 1;
            while (!irc.Connected)
            {
                Console.WriteLine("Connect attempt " + count);
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
                    Console.WriteLine("Unable to connect. Retrying in 5 seconds");
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Error Message (via Connect()): {0}", e);
                }
                count++;
                // Console.WriteLine("Connection failed.  Retrying in 5 seconds.");
                Thread.Sleep(5000);
            }
            StartThreads();
        }

        private void StartThreads()
        {
            listener = new Thread(new ThreadStart(Listen));
            listener.Start();

            timerThread = new Thread(new ThreadStart(doWork));
            timerThread.Start();

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
            print(message);
        }

        private void addUserToList(String nick)
        {
            lock (users)
            {
                if (!users.Contains(nick))
                {
                    users.Add(nick);
                }
            }
        }

        private void removeUserFromList(String nick)
        {
            lock (users)
            {
                if (users.Contains(nick))
                {
                    users.Remove(nick);
                }
            }
        }

        private void buildUserList()
        {
            sendRaw("WHO " + channel);
        }

        private String capName(String user)
        {
            return char.ToUpper(user[0]) + user.Substring(1);
        }

        private String getUser(String message)
        {
            String[] temp = message.Split('!');
            user = temp[0].Substring(1);
            return capName(user);
        }

        private void setNick(String tNick)
        {
            nick = tNick.ToLower();
        }

        private void setPassword(String tPassword)
        {
            password = tPassword;
        }

        private void setChannel(String tChannel)
        {
            if (tChannel.StartsWith("#"))
            {
                channel = tChannel;
            }
            else
            {
                channel = "#" + tChannel;
            }
        }

        private void setAdmin(String tChannel)
        {
            if (tChannel.StartsWith("#"))
            {
                admin = tChannel.Substring(1);
            }
            else
            {
                admin = tChannel;
            }
        }

        private void print(String message)
        {
            Console.WriteLine(message);
        }

        private void sendRaw(String message)
        {

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

        private void sendMessage(String message, int priority)
        {
            if (priority == 1)
            {
                highPriority.Enqueue(message);
            }
            else if (priority == 2)
            {
                normalPriority.Enqueue(message);
            }
            else lowPriority.Enqueue(message);
        }

        private bool checkStream()
        {
            if (irc.Connected)
            {
                using (var w = new WebClient())
                {
                    String json_data = "";
                    try
                    {
                        w.Proxy = null;
                        json_data = w.DownloadString("https://api.twitch.tv/kraken/streams/" + channel.Substring(1));
                        JObject stream = JObject.Parse(json_data);
                        if (stream["stream"].HasValues)
                        {
                            //print("STREAM ONLINE");
                            return true;
                        }
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("Unable to connect to twitch API to check stream status. Skipping.");
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("Error Message (via checkStream()): {0}", e);
                    }
                }

                //print("STREAM OFFLINE");
                return false;
            }
            return false;
        }

        private void doWork()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(60000);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("No response from twitch.  Skipping handout.");
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Error Message (via doWork()): {0}", e);
                }
            }
        }

        private void addToLookups(String nick)
        {
            if (!usersToLookup.Contains(nick))
            {
                usersToLookup.Add(nick);
            }
        }

        private void handleMessageQueue(Object state)
        {
            String message;
            //Console.WriteLine("Entering Message Queue.  Time: " + DateTime.Now);
            if (highPriority.TryDequeue(out message))
            {
                print(nick + ": " + message);
                sendRaw("PRIVMSG " + channel + " :" + message);
                messageQueue.Change(4000, Timeout.Infinite);
            }
            else if (normalPriority.TryDequeue(out message))
            {
                print(nick + ": " + message);
                sendRaw("PRIVMSG " + channel + " :" + message);
                messageQueue.Change(4000, Timeout.Infinite);
            }
            else if (lowPriority.TryDequeue(out message))
            {
                print(nick + ": " + message);
                sendRaw("PRIVMSG " + channel + " :" + message);
                messageQueue.Change(4000, Timeout.Infinite);
            }
            else messageQueue.Change(4000, Timeout.Infinite);
        }

        public void Dispose()
        {
            if (listener != null) listener.Abort();
            if (timerThread != null) timerThread.Abort();
            if (KAthread != null) KAthread.Abort();
        }
    }
}
