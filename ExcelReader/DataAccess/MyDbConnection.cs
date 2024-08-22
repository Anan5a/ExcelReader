using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class MyDbConnection:IMyDbConnection
    {
        public string ConnectionString { get; set; }
        public MyDbConnection(string connectionString) 
        {
            ConnectionString = connectionString;
        }
    }
}
