using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MiniORM
{
    public class DatabaseFacade : IDatabaseFacade
    {
        private IDbProvider _dbProvider;
        private IORMProvider _ormProvider;

        public event EventHandler<ConnectionCreatedEventArgs> ConnectionCreated
        {
            add { _dbProvider.ConnectionCreated += value; }
            remove { _dbProvider.ConnectionCreated -= value; }
        }
        public event EventHandler<ConnectedEventArgs> Connected
        {
            add { _dbProvider.Connected += value; }
            remove { _dbProvider.Connected -= value; }
        }
        public event EventHandler<ExecutedEventArgs> Executed
        {
            add { _dbProvider.Executed += value; }
            remove { _dbProvider.Executed -= value; }
        }
        public event EventHandler<LoadDataCompletedEventArgs> LoadDataCompleted
        {
            add { _dbProvider.LoadDataCompleted += value; }
            remove { _dbProvider.LoadDataCompleted -= value; }
        }
        public event EventHandler<TransactionStartedEventArgs> TransactionStarted
        {
            add { _dbProvider.TransactionStarted += value; }
            remove { _dbProvider.TransactionStarted -= value; }
        }
        public event EventHandler<InsertCompletedEventArgs> InsertCompleted
        {
            add { _ormProvider.InsertCompleted += value; }
            remove { _ormProvider.InsertCompleted -= value; }
        }
        public event EventHandler<DeleteCompletedEventArgs> DeleteCompleted
        {
            add { _ormProvider.DeleteCompleted += value; }
            remove { _ormProvider.DeleteCompleted -= value; }
        }
        public event EventHandler<UpdateCompletedEventArgs> UpdateCompleted
        {
            add { _ormProvider.UpdateCompleted += value; }
            remove { _ormProvider.UpdateCompleted -= value; }
        }

        public DatabaseFacade(IDbProvider dbProvider, IORMProvider ormProvider)
        {
            _dbProvider = dbProvider;
            _ormProvider = ormProvider;
        }

        public IDatabaseFacade CreateConnection(Connection connection)
        {
            _dbProvider.CreateConnectionSql(connection);
            return this;
        }

        public IDbProvider CreateConnectionSql(Connection connection)
        {
            _dbProvider.CreateConnectionSql(connection);
            return _dbProvider;
        }

        public int? Execute(string query, object sqlParam = null, CommandType cmdType = CommandType.Text)
        {
            return _dbProvider.Execute(query, sqlParam, cmdType);
        }

        public async Task<int?> ExecuteAsync(string query, dynamic sqlParam = null, CommandType cmdType = CommandType.Text)
        {
            return await _dbProvider.ExecuteAsync(query, sqlParam, cmdType);
        }

        public int? Insert<TEntity>(TEntity param) where TEntity : class, new()
        {
            return _ormProvider.Insert(param);
        }

        public int? Insert<TEntity>(IEnumerable<TEntity> param) where TEntity : class, new()
        {
            return _ormProvider.Insert(param);
        }

        public async Task<int?> InsertAsync<TEntity>(TEntity param) where TEntity : class, new()
        {
            return await _ormProvider.InsertAsync(param);
        }

        public async Task<int?> InsertAsync<TEntity>(IEnumerable<TEntity> param) where TEntity : class, new()
        {
            return await _ormProvider.InsertAsync(param);
        }

        public IEnumerable<TEntity> LoadData<TEntity>(string query, dynamic sqlParam = null, Func<IDataReader, TEntity> drHandler = null, CommandType cmdType = CommandType.Text) where TEntity : class, new()
        {
            return _dbProvider.LoadData<TEntity>(query, sqlParam, drHandler, cmdType);
        }

        public async Task<IEnumerable<TEntity>> LoadDataAsync<TEntity>(string query, dynamic sqlParam = null, Func<IDataReader, TEntity> drHandler = null, CommandType cmdType = CommandType.Text) where TEntity : class, new()
        {
            return await _dbProvider.LoadDataAsync<TEntity>(query, sqlParam, drHandler, cmdType);
        }

        public object LoadDataReader(string query, dynamic sqlParam = null, Action<IDataReader> drHandler = null, CommandType cmdType = CommandType.Text)
        {
            return _dbProvider.LoadDataReader(query, sqlParam, drHandler, cmdType);
        }

        public async Task<object> LoadDataReaderAsync<TEntity>(string query, dynamic sqlParam = null, Action<IDataReader> drHandler = null, CommandType cmdType = CommandType.Text) where TEntity : class, new()
        {
            return await _dbProvider.LoadDataReaderAsync<TEntity>(query, sqlParam, drHandler, cmdType);
        }

        public object LoadSingleItem(string query, dynamic sqlParam = null, CommandType cmdType = CommandType.Text)
        {
            return _dbProvider.LoadSingleItem(query, sqlParam, cmdType);
        }

        public async Task<object> LoadSingleItemAsync(string query, dynamic sqlParam = null, CommandType cmdType = CommandType.Text)
        {
            return await _dbProvider.LoadSingleItemAsync(query, sqlParam, cmdType);
        }

        public async Task<bool> TestConnectionAsync(Action<object> callback = null)
        {
            return await _dbProvider.TestConnectionAsync(callback);
        }

        public async Task<bool> TransactionAsync(Action action)
        {
            return await _dbProvider.TransactionAsync(action);
        }

        public int? Update<TEntity>(TEntity param)
          where TEntity : class, new()
        {
            return _ormProvider.Update(param);
        }

        public async Task<int?> UpdateAsync<TEntity>(TEntity param) where TEntity : class, new()
        {
            return await _ormProvider.UpdateAsync(param);
        }

        public int? Delete<TEntity>(TEntity param)
            where TEntity : class, new()
        {
            return _ormProvider.Delete(param);
        }

        public async Task<int?> DeleteAsync<TEntity>(TEntity param)
           where TEntity : class, new()
        {
            return await _ormProvider.DeleteAsync(param).ConfigureAwait(false);
        }
    }
}
