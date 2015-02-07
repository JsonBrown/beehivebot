using beehive.common.Contracts;
using beehive.common.Objects;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.core.Processors
{
    public class IrcMessageResultProcessor : IResultProcessor
    {
        private ILog log = LogManager.GetLogger(typeof(IrcMessageResultProcessor));
        private readonly StreamWriter write;
        private readonly string channel;
        public IrcMessageResultProcessor(string channel, StreamWriter write)
        {
            this.channel = channel;
            this.write = write;
        }
        public void Process(CommandResult result)
        {
            var message = String.Format("PRIVMSG {0} : {1}", channel, result.Message);
            log.DebugFormat("Sending message: {0}", message);
            write.WriteLine(message);
        }

        public void Dispose() {}
    }
}
