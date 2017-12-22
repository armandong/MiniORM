using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public interface IQueryBuilder 
    {
        string CreateInsertStatement<TEntity>(IEnumerable<TEntity> param) 
            where TEntity : class;

        string CreateDeleteStatement<TEntity>(TEntity param)
            where TEntity : class;
        string CreateUpdateStatement<TEntity>(TEntity param)
            where TEntity : class;
    }
}
