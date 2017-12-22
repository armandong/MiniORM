using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class ORM
    {
        private IDatabaseFacade _databaseFacade;

        public ORM()
        {
            IEntityMapper mapper = new EntityMapper();
            IDbProvider dbProvider = new MySqlService(mapper);
            IQueryBuilder queryBuilder = new MainQueryBuilder(dbProvider, mapper);
            IORMProvider ormProvider = new ORMProvider(dbProvider, queryBuilder);
            _databaseFacade = new DatabaseFacade(dbProvider, ormProvider);
        }

        public IDatabaseFacade CreateConnection(Connection connection)
        {
            return _databaseFacade.CreateConnection(connection);
        }
    }
}
