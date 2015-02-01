using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.data
{
    public interface IContext : IDisposable
    {
        IQueryable<T> Get<T>() where T : class;
        void Add<T>(T item) where T : class;
        void Delete(object item);
        void Save();
    }
}
