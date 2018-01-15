using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class ORM
    {
        private DbConnection _dbConnection;

        private IMiniORM _databaseFacade;

        private MapperType _mapperType;
        private QueryBuilderType _qbType;
        private ORMProviderType _ormProviderType;

        public ORM(
            DbConnection dbConnection,
            MapperType mapperType = MapperType.DEFAULT, 
            QueryBuilderType qbType = QueryBuilderType.DEFAULT,
            ORMProviderType ormProviderType = ORMProviderType.DEFAULT)
        {
            _dbConnection = dbConnection;
            _mapperType = mapperType;
            _qbType = qbType;
            _ormProviderType = ormProviderType;
        }

        private void _Build()
        {
            IEntityMapper mapper = null;
            IDbProvider dbProvider = null;
            IQueryBuilder queryBuilder = null;
            IORMProvider ormProvider = null;

            if (_mapperType == MapperType.DEFAULT)
                mapper = new EntityMapper();

            dbProvider = new SqlService(mapper);

            if (_qbType == QueryBuilderType.DEFAULT)
                queryBuilder = new MainQueryBuilder(dbProvider, mapper);

            if (_ormProviderType == ORMProviderType.DEFAULT)
                ormProvider = new ORMProvider(dbProvider, queryBuilder);

            _databaseFacade = new DatabaseFacade(dbProvider, ormProvider);
        }

        public IMiniORM Build()
        {
            _Build();
            return _databaseFacade.CreateConnection(_dbConnection);
        }
    }
}
