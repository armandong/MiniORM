using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class InsertCompletedEventArgs : EventArgs
    {
        public IEnumerable<object> resultList { get; set; }
    }
}
