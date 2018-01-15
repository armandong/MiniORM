using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public interface IDbProvider : IDbProviderEvents
    {
        IDbProvider CreateConnectionSql(Connection connection);
        Task<bool> TestConnectionAsync(Action<object> callback = null);

        int? Execute(string query, object sqlParam = null, CommandType cmdType = CommandType.Text);
        Task<int?> ExecuteAsync(string query, dynamic sqlParam = null, CommandType cmdType = CommandType.Text);

        IEnumerable<TEntity> LoadData<TEntity>(string query, dynamic sqlParam = null, Func<IDataReader, TEntity> drHandler = null, CommandType cmdType = CommandType.Text)
            where TEntity : class, new();
        Task<IEnumerable<TEntity>> LoadDataAsync<TEntity>(string query, dynamic sqlParam = null, Func<IDataReader, TEntity> drHandler = null, CommandType cmdType = CommandType.Text)
                            where TEntity : class, new();

        object LoadSingleItem(string query, dynamic sqlParam = null,
                               CommandType cmdType = CommandType.Text);
        Task<object> LoadSingleItemAsync(string query, dynamic sqlParam = null,
                                 CommandType cmdType = CommandType.Text);

        object LoadDataReader(string query, dynamic sqlParam = null, Action<IDataReader> drHandler = null, CommandType cmdType = CommandType.Text);
        Task<object> LoadDataReaderAsync<TEntity>(string query, dynamic sqlParam = null, Action<IDataReader> drHandler = null, CommandType cmdType = CommandType.Text)
            where TEntity : class, new();

        Task<bool> TransactionAsync(Action action);
    }
}
