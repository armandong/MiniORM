using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public interface IORMProvider : IORMProviderEvents
    {
        Task<int?> InsertAsync<TEntity>(TEntity param) where TEntity : class, new();
        Task<int?> InsertAsync<TEntity>(IEnumerable<TEntity> param) where TEntity : class, new();

        int? Insert<TEntity>(TEntity param)
            where TEntity : class, new();
        int? Insert<TEntity>(IEnumerable<TEntity> param)
            where TEntity : class, new();

        int? Update<TEntity>(TEntity param)
            where TEntity : class, new();
        Task<int?> UpdateAsync<TEntity>(TEntity param)
            where TEntity : class, new();

        int? Delete<TEntity>(TEntity param)
            where TEntity : class, new();
        Task<int?> DeleteAsync<TEntity>(TEntity param)
            where TEntity : class, new();
    }
}
