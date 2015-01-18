using beehive.common.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.core.External
{
    public class LocalDisk : IDisk
    {
        private readonly string root;
        public LocalDisk(string root)
        {
            this.root = root;
        }
        public List<string> GetFiles(string path)
        {
            path = String.Format("{0}{1}", root, path);
            return Directory.Exists(path) ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).ToList() : new List<string>();
        }

        public Stream Read(string path)
        {
            path = String.Format("{0}{1}",root, path);
            return File.OpenRead(path);
        }

        public bool Exists(string path)
        {
            path = String.Format("{0}{1}", root, path);
            return File.Exists(path);
        }

        public void Delete(string path)
        {
            path = String.Format("{0}{1}", root, path);
            File.Delete(path);
        }

        public List<string> GetDirectories(string path)
        {
            path = String.Format("{0}{1}", root, path);
            return Directory.Exists(path) ? Directory.GetDirectories(path).ToList() : new List<string>();
        }
    }
}
