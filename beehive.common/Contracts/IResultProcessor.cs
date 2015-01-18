using beehive.common.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.common.Contracts
{
    public interface IResultProcessor
    {
        void Process(CommandResult result);
    }
}
