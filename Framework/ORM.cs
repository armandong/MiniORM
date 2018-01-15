using MySql.Data.MySqlClient;
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

        private MapperType _mapperType = MapperType.DEFAULT;
        private DbProviderType _dbProviderType = DbProviderType.MySql;
        private QueryBuilderType _qbType = QueryBuilderType.DEFAULT;
        private ORMProviderType _ormProviderType = ORMProviderType.DEFAULT;

        public ORM()
        {

        }

        public ORM SetMapper(MapperType mapperType)
        {
            _mapperType = mapperType;
            return this;
        }

        public ORM SetDbProvider(DbProviderType dbProviderType)
        {
            _dbProviderType = dbProviderType;
            return this;
        }

        public ORM SetQueryBuilder(QueryBuilderType qbType)
        {
            _qbType = qbType;
            return this;
        }

        public ORM SetORM(ORMProviderType ormProviderType)
        {
            _ormProviderType = ormProviderType;
            return this;
        }

        private void _Build()
        {
            IEntityMapper mapper = null;
            IDbProvider dbProvider = null;
            IQueryBuilder queryBuilder = null;
            IORMProvider ormProvider = null;

            if (_mapperType == MapperType.DEFAULT)
                mapper = new EntityMapper();

            if (_dbProviderType == DbProviderType.MySql)
            {
                dbProvider = new SqlService(mapper, (query, conn) =>
                {
                    MySqlConnection connection = conn as MySqlConnection;

                    return new MySqlCommand(query, connection);
                }, (connection) =>
                {
                    string _connectionString = string.Format("server={0};database={1};uid={2};password={3}",
                    connection.Host, connection.Database, connection.User, connection.Password);

                    return new MySqlConnection(_connectionString);
                });
            }

            if (_qbType == QueryBuilderType.DEFAULT)
                queryBuilder = new MainQueryBuilder(dbProvider, mapper);

            if (_ormProviderType == ORMProviderType.DEFAULT)
                ormProvider = new ORMProvider(dbProvider, queryBuilder);

            _databaseFacade = new DatabaseFacade(dbProvider, ormProvider);
        }

        public IDatabaseFacade CreateConnection(Connection connection)
        {
            _Build();
            return _databaseFacade.CreateConnection(connection);
        }
    }
}
