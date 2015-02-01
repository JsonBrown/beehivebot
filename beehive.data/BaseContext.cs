using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.data
{
    public class BaseContext : DbContext, IContext
    {
        public BaseContext(DbConnection conn, bool owned)
            : base(conn, owned)
        { }
        public IQueryable<T> Get<T>() where T : class
        {
            return (IQueryable<T>)this.Set<T>();
        }
        public void Save()
        {
            this.SaveChanges();
        }
        void IDisposable.Dispose()
        {
            this.Dispose();
        }
        
        public void Add<T>(T item) where T : class
        {
            this.Set<T>().Add(item);
        }

        public void Delete(object item)
        {
            ((IObjectContextAdapter)this).ObjectContext.DeleteObject(item);
        }
    }    
}
