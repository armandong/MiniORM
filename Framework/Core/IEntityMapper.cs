using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public interface IEntityMapper
    {
        TEntity Map<TEntity>(IDataReader reader, List<string> columns) where TEntity : class, new();

        string MapColumn(PropertyInfo prop, List<string> columns);

        string MapTable(Type entityType, List<string> tableList);
    }
}
