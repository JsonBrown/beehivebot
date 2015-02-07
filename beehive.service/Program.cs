using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace beehive.service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            var log = LogManager.GetLogger(typeof(Program));
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new BeeHiveBotService() 
            };
            try
            {
                ServiceBase.Run(ServicesToRun);
            } catch (Exception e)
            {
                log.Error(e);
            }
            
        }
    }
}
