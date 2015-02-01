using beehive.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using beehive.core.External;
using System.Reflection;
using System.IO;
using beehive.data;

namespace beehive.console
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var disk = new LocalDisk(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName);

            using (var data = new BeehiveContext(ConfigurationManager.ConnectionStrings["BeeHive"].ConnectionString))
            using (var irc = new BeeHiveBot(ConfigurationManager.AppSettings["botName"], String.Format("oauth:{0}", ConfigurationManager.AppSettings["botToken"]), ConfigurationManager.AppSettings["channel"], disk, data))
            {
                Console.ReadLine();
            }
            
        }
    }
}
