﻿using beehive.common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.common.Objects
{
    public class CommandResult
    {
        public CommandResult(Priority queue, string message, string processor)
        {
            Queue = queue;
            Message = message;
            Processor = processor;
        }
        public Priority Queue { get; set; }
        public string Processor { get; set; }
        public string Message { get; set; }
    }
}