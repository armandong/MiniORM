using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class ExecutedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public object LastInsertedId { get; set; }
        public ExecuteType ExecuteType { get; set; }
    }
}
