using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public static class MiniORMExtension
    {
        public static IMiniORM MiniORM(this DbConnection connection, 
            MapperType mapperType = MapperType.DEFAULT,
            QueryBuilderType qbType = QueryBuilderType.DEFAULT,
            ORMProviderType ormProviderType = ORMProviderType.DEFAULT)
        {
            ORM orm = new ORM(connection, mapperType, qbType, ormProviderType);

            return orm.Build();
        }
    }
}
