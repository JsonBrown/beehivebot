using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using log4net;



namespace beehive.core
{
    public class Irc : IDisposable
    {
        private ILog log = LogManager.GetLogger(typeof(Irc));
        private string admin, password, nick, channel;
        private TcpClient irc;
        
        private int attempt;
        public Irc(string nick, string password, string channel)
        {
            this.nick = nick.ToLower();
            this.password = password;
            this.channel =  channel;
            this.admin = !channel.StartsWith("#") ? channel : channel.Substring(1);

            Connect();
        }

        public StreamReader Read { get; set; }
        public StreamWriter Write { get; set; }
        public bool Connected
        {
            get
            {
                return (irc != null && irc.Connected);
            }
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

                    Read = new StreamReader(irc.GetStream());
                    Write = new StreamWriter(irc.GetStream());

                    Write.AutoFlush = true;

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

        
        private void sendRaw(String message)
        {
            log.DebugFormat("Sending message: {0}", message);
            try
            {
                Write.WriteLine(message);
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

        public void Dispose()
        {
            if (irc != null) irc.Close();

        }
    }
}
