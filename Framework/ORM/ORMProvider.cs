using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class ORMProvider : IORMProvider
    {
        private IDbProvider _dbProvider;
        private IQueryBuilder _queryBuilder;

        public ORMProvider(IDbProvider dbProvider, IQueryBuilder queryBuilder)
        {
            _dbProvider = dbProvider;
            _queryBuilder = queryBuilder;
        }

        public int? Insert<TEntity>(TEntity param)
            where TEntity : class, new()
        {
            List<TEntity> entityList = new List<TEntity>();
            entityList.Add(param);

            return Insert(entityList as IEnumerable<TEntity>);
        }

        public int? Insert<TEntity>(IEnumerable<TEntity> param)
            where TEntity : class, new()
        {
            string query = _queryBuilder.CreateInsertStatement(param);

            return _dbProvider.Execute(query);
        }

        public async Task<int?> InsertAsync<TEntity>(TEntity param) where TEntity : class, new()
        {
            List<TEntity> entityList = new List<TEntity>();
            entityList.Add(param);

            return await InsertAsync(entityList as IEnumerable<TEntity>);
        }

        public async Task<int?> InsertAsync<TEntity>(IEnumerable<TEntity> param) where TEntity : class, new()
        {
            string query = _queryBuilder.CreateInsertStatement(param);

            return await _dbProvider.ExecuteAsync(query).ConfigureAwait(false);
        }

        public int? Update<TEntity>(TEntity param)
          where TEntity : class, new()
        {
            string query = _queryBuilder.CreateUpdateStatement(param);

            return _dbProvider.Execute(query);
        }

        public async Task<int?> UpdateAsync<TEntity>(TEntity param)
           where TEntity : class, new()
        {
            string query = _queryBuilder.CreateUpdateStatement(param);

            return await _dbProvider.ExecuteAsync(query).ConfigureAwait(false);
        }

        public int? Delete<TEntity>(TEntity param)
            where TEntity : class, new()
        {
            string query = _queryBuilder.CreateDeleteStatement(param);

            return _dbProvider.Execute(query);
        }

        public async Task<int?> DeleteAsync<TEntity>(TEntity param)
           where TEntity : class, new()
        {
            string query = _queryBuilder.CreateDeleteStatement(param);

            return await _dbProvider.ExecuteAsync(query).ConfigureAwait(false);
        }
    }
}
