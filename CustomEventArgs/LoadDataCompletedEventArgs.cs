using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class LoadDataCompletedEventArgs : EventArgs
    {
        public IEnumerable<object> DataList { get; set; }
        public bool Success { get; set; }
    }
}
