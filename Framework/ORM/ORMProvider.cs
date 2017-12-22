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

        public event EventHandler<InsertCompletedEventArgs> InsertCompleted;
        public event EventHandler<DeleteCompletedEventArgs> DeleteCompleted;
        public event EventHandler<UpdateCompletedEventArgs> UpdateCompleted;

        public void OnInsertCompleted(IEnumerable<object> resultList, bool success, string queryString)
        {
            InsertCompleted?.Invoke(this, new InsertCompletedEventArgs { ResultList = resultList, Success = success, QueryString = queryString });
        }

        public void OnDeleteCompleted(bool success, string queryString)
        {
            DeleteCompleted?.Invoke(this, new DeleteCompletedEventArgs { Success = success, QueryString = queryString });
        }

        public void OnUpdateCompleted(bool success, string queryString)
        {
            UpdateCompleted?.Invoke(this, new UpdateCompletedEventArgs { Success = success, QueryString = queryString });
        }

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

            OnInsertCompleted(param, true, query);

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

            OnInsertCompleted(param, true, query);

            return await _dbProvider.ExecuteAsync(query).ConfigureAwait(false);
        }

        public int? Update<TEntity>(TEntity param)
          where TEntity : class, new()
        {
            string query = _queryBuilder.CreateUpdateStatement(param);

            OnUpdateCompleted(true, query);

            return _dbProvider.Execute(query);
        }

        public async Task<int?> UpdateAsync<TEntity>(TEntity param)
           where TEntity : class, new()
        {
            string query = _queryBuilder.CreateUpdateStatement(param);

            OnUpdateCompleted(true, query);

            return await _dbProvider.ExecuteAsync(query).ConfigureAwait(false);
        }

        public int? Delete<TEntity>(TEntity param)
            where TEntity : class, new()
        {
            string query = _queryBuilder.CreateDeleteStatement(param);

            OnDeleteCompleted(true, query);

            return _dbProvider.Execute(query);
        }

        public async Task<int?> DeleteAsync<TEntity>(TEntity param)
           where TEntity : class, new()
        {
            string query = _queryBuilder.CreateDeleteStatement(param);

            OnDeleteCompleted(true, query);

            return await _dbProvider.ExecuteAsync(query).ConfigureAwait(false);
        }
    }
}
