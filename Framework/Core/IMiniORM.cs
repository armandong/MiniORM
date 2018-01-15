using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public interface IMiniORM : IDbProvider, IORMProvider
    {
         IMiniORM CreateConnection(DbConnection connection);
    }
}
