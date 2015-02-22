using beehive.data.model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.data
{
    public class BeehiveContext : BaseContext
    {
        public BeehiveContext(string connectionString)
            : base(new SQLiteConnection() { ConnectionString = connectionString }, true)
        { }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomCommand>();
            modelBuilder.Entity<Quote>();
        }
    }
}
