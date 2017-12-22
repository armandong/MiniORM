using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public interface IDatabaseFacade : IDbProvider, IORMProvider
    {
         IDatabaseFacade CreateConnection(Connection connection);
    }
}
