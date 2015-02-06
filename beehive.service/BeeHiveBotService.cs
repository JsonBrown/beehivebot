﻿using beehive.core;
using beehive.core.External;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace beehive.service
{
    public partial class BeeHiveBotService : ServiceBase
    {
        private BeeHiveBot bot;
        public BeeHiveBotService()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
        }

        protected override void OnStart(string[] args)
        {
            var disk = new LocalDisk(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName);
            bot = new BeeHiveBot(ConfigurationManager.AppSettings["botName"], String.Format("oauth:{0}", ConfigurationManager.AppSettings["botToken"]), ConfigurationManager.AppSettings["channel"], disk);
        }

        protected override void OnStop()
        {
            bot.Dispose();
        }
    }
}
