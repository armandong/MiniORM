using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class UpdateCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; } = false;
        public string QueryString { get; set; }
    }
}
