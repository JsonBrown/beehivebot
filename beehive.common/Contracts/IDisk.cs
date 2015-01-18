using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.common.Contracts
{
    public interface IDisk
    {
        List<string> GetFiles(string path);
        List<string> GetDirectories(string path);
        Stream Read(string path);
        bool Exists(string path);
        void Delete(string path);
    }
}
