using beehive.common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.common.Objects
{
    public class IRCMessage
    {
        public IRCMessage(Priority queue, string message)
        {
            Queue = queue;
            Message = message;
        }
        public Priority Queue { get; set; }
        public string Message { get; set; }
    }
}
