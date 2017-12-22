using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DbOptionAttribute : Attribute
    {
        public DbGenerateOption Option { get; set; }
        public DbOptionAttribute(DbGenerateOption _option)
        {
            Option = _option;
        }
    }
}
