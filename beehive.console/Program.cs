using beehive.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace beehive.console
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            using (var irc = new Irc(ConfigurationManager.AppSettings["botName"], String.Format("oauth:{0}", ConfigurationManager.AppSettings["botToken"]), ConfigurationManager.AppSettings["channel"]))
            {
                Console.ReadLine();
            }
        }
    }
}
