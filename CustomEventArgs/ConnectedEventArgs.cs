using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class ConnectedEventArgs : EventArgs
    {
        public DbProviderType ProviderType { get; set; }
    }
}
