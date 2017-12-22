using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class ConnectionCreatedEventArgs : EventArgs
    {
        public Connection ConnectionDetails { get; set; }
    }
}
