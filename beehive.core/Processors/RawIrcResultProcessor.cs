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
    public class RawIrcResultProcessor : IResultProcessor
    {
        private ILog log = LogManager.GetLogger(typeof(RawIrcResultProcessor));
        private readonly StreamWriter write;
        public RawIrcResultProcessor(StreamWriter write)
        {
            this.write = write;
        }
        public void Process(CommandResult result)
        {
            log.DebugFormat("Sending message: {0}", result.Message);
            write.WriteLine(result.Message);
        }
    }
}
