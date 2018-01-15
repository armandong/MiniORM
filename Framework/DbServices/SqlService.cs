using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class SqlService : IDbProvider
    {
        private DbConnection _con;
        private bool _transactionIsActive = false;
        private IEntityMapper _mapper;

        public event EventHandler<ConnectionCreatedEventArgs> ConnectionCreated;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<ExecutedEventArgs> Executed;
        public event EventHandler<LoadDataCompletedEventArgs> LoadDataCompleted;
        public event EventHandler<TransactionStartedEventArgs> TransactionStarted;

        public void OnConnectionCreated()
        {
            ConnectionCreated?.Invoke(this, new ConnectionCreatedEventArgs {  });
        }

        public void OnConnected()
        {
            Connected?.Invoke(this, new ConnectedEventArgs { });
        }

        public void OnExecuted(bool success, object lastInsertedId, ExecuteType executeType)
        {
            Executed?.Invoke(this, new ExecutedEventArgs { Success = success, LastInsertedId = lastInsertedId, ExecuteType = executeType });
        }

        public void OnLoadDataCompleted(IEnumerable<object> dataList, bool success)
        {
            LoadDataCompleted?.Invoke(this, new LoadDataCompletedEventArgs { DataList = dataList, Success = success });
        }

        public void OnTransactionStarted()
        {
            TransactionStarted?.Invoke(this, new TransactionStartedEventArgs { });
        }

        public SqlService(IEntityMapper mapper)
        {
            _mapper = mapper;
        }

        #region PUBLIC

        #region SYNCHRONOUS METHODS
        public IDbProvider CreateConnectionSql(DbConnection connection)
        {
            _con = connection;

            OnConnectionCreated();

            return this;
        }

        public int? Execute(string query, object sqlParam = null, CommandType cmdType = CommandType.Text)
        {
            return _SqlTryCatch(con =>
            {
                using (DbCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = query;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam, cmd));

                    cmd.ExecuteNonQueryAsync();

                    ExecuteType executeType = MiniOrmHelpers.CheckExecuteType(query);

                    //int lastInsertId = Convert.ToInt32(cmd.LastInsertedId);
                    int lastInsertId = 0;
                    OnExecuted(true, lastInsertId, executeType);

                    return lastInsertId;
                }
            }, error =>
            {
                ExecuteType executeType = MiniOrmHelpers.CheckExecuteType(query);
                OnExecuted(false, 0, executeType);
            });
        }

        public IEnumerable<TEntity> LoadData<TEntity>(string query, dynamic sqlParam = null, Func<IDataReader, TEntity> drHandler = null, CommandType cmdType = CommandType.Text)
            where TEntity : class, new()
        {
            return _SqlTryCatch((con) =>
            {
                List<TEntity> entityListTemp = new List<TEntity>();
                using (DbCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = query;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam, cmd));

                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        List<string> columns = Enumerable.Range(0, dr.FieldCount).Select(dr.GetName).ToList();

                        while (dr.Read())
                        {
                            if (drHandler != null)
                                entityListTemp.Add(drHandler(dr));
                            else
                                entityListTemp.Add(_mapper.Map<TEntity>(dr, columns));
                        }
                    }
                }

                OnLoadDataCompleted(entityListTemp, true);
                return entityListTemp;
            }, exceptionMessage =>
            {
                OnLoadDataCompleted(null, false);
            });
        }

        public object LoadDataReader(string query, dynamic sqlParam = null, Action<IDataReader> drHandler = null, CommandType cmdType = CommandType.Text)
        {
            return _SqlTryCatch<object>((con) =>
            {
                using (DbCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = query;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam, cmd));

                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            drHandler?.Invoke(dr);
                        }
                    }
                }

                return null;
            });
        }

        public object LoadSingleItem(string query, dynamic sqlParam = null,
                                CommandType cmdType = CommandType.Text)
        {
            return _SqlTryCatch((con) =>
            {
                using (DbCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = query;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(sqlParam);

                    return cmd.ExecuteScalar();
                }
            });
        }

        #endregion

        #region ASYNCHRONOUS METHODS
        public async Task<bool> TestConnectionAsync(Action<object> callback = null)
        {
            DbConnection con = _GetConnection();

            try
            {
                await con.OpenAsync().ConfigureAwait(false);
                callback?.Invoke(null);
            }
            catch (Exception ex)
            {
                callback?.Invoke(ex.Message);
                return false;
            }
            finally
            {
                if (!_transactionIsActive)
                    con.Close();
            }

            OnConnected();
            return true;
        }

        public async Task<int?> ExecuteAsync(string query, object sqlParam = null, CommandType cmdType = CommandType.Text)
        {
            return await _SqlTryCatchAsync(async (con) =>
            {
                using (DbCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = query;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam, cmd));

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    ExecuteType executeType = MiniOrmHelpers.CheckExecuteType(query);
                    
                    //int lastInsertId = Convert.ToInt32(cmd.LastInsertedId);
                    int lastInsertId = 0;

                    OnExecuted(true, lastInsertId, executeType);

                    return lastInsertId;
                }
            }, error =>
            {
                ExecuteType executeType = MiniOrmHelpers.CheckExecuteType(query);
                OnExecuted(false, 0, executeType);
            });
        }

        public async Task<IEnumerable<TEntity>> LoadDataAsync<TEntity>(string query, dynamic sqlParam = null, Func<IDataReader, TEntity> drHandler = null, CommandType cmdType = CommandType.Text)
            where TEntity : class, new()
        {

            return await _SqlTryCatchAsync(async (con) =>
            {
                List<TEntity> entityListTemp = new List<TEntity>();
                using (DbCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = query;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam, cmd));

                    using (IDataReader dr = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        List<string> columns = Enumerable.Range(0, dr.FieldCount).Select(dr.GetName).ToList();

                        while (dr.Read())
                        {
                            if (drHandler != null)
                                entityListTemp.Add(drHandler(dr));
                            else
                                entityListTemp.Add(_mapper.Map<TEntity>(dr, columns));
                        }
                    }
                }

                OnLoadDataCompleted(entityListTemp, true);
                return entityListTemp;
            });
        }

        public async Task<object> LoadDataReaderAsync<TEntity>(string query, dynamic sqlParam = null, Action<IDataReader> drHandler = null, CommandType cmdType = CommandType.Text)
            where TEntity : class, new()
        {

            return await _SqlTryCatchAsync(async (con) =>
            {
                using (DbCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = query;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam, cmd));

                    using (IDataReader dr = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (dr.Read())
                        {
                            drHandler(dr);
                        }
                    }
                }

                return new object();
            });
        }

        public async Task<object> LoadSingleItemAsync(string query, dynamic sqlParam = null,
                                CommandType cmdType = CommandType.Text)
        {
            return await _SqlTryCatchAsync(async (con) =>
            {
                using (DbCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = cmdType;
                    cmd.CommandText = query;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(sqlParam);

                    return await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                }
            });
        }

        public async Task<bool> TransactionAsync(Action action)
        {
            DbTransaction trans = null;
            try
            {
                using (DbConnection con = _GetConnection())
                {
                    await con.OpenAsync().ConfigureAwait(false);
                    trans = con.BeginTransaction();

                    OnTransactionStarted();

                    _transactionIsActive = true;
                    action?.Invoke();

                    trans.Commit();

                    con.Disposed += (s, e) => { _transactionIsActive = false; };
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return false;
            }

            return true;
        }

        #endregion

        #endregion

        #region PRIVATE

        #region SYNCHRONOUS METHODS
        private DbConnection _GetConnection()
        {
            return _con;
        }

        private TReturn _SqlTryCatch<TReturn>(Func<DbConnection, TReturn> func, Action<string> exceptionHandler = null)
        {
            DbConnection con = _GetConnection();

            try
            {
                if (!_transactionIsActive)
                    con.Open();

                return func(con);
            }
            catch (Exception ex)
            {
                if (_transactionIsActive)
                    throw new Exception(ex.Message);
                exceptionHandler(ex.Message);
                return default(TReturn);
            }
            finally
            {
                if (!_transactionIsActive)
                    con.Close();
            }
        }
        private DbParameter[] _SetParams(object param, DbCommand command)
        {
            PropertyInfo[] props = param.GetType().GetProperties() as PropertyInfo[];
            List<DbParameter> sqlParams = new List<DbParameter>();

            foreach (PropertyInfo prop in props)
            {
                string propName = prop.Name;
                DbParameter parameter = command.CreateParameter();
                parameter.ParameterName = "@" + propName;
                parameter.Value = prop.GetValue(param, null);

                sqlParams.Add(parameter);
            }

            return sqlParams.ToArray();
        }


        //private DbParameterCollection[] _SetParamsExecute(object param, string query = "")
        //{
        //    if (!(param is Array))
        //        return _SetParams(param);

        //    List<MySqlParameter> sqlParams = new List<MySqlParameter>();

        //    foreach (dynamic item in (dynamic[])param)
        //    {
        //        foreach (PropertyInfo prop in item.GetType().GetProperties() as PropertyInfo[])
        //        {

        //        }
        //    }

        //    throw new NotImplementedException();
        //}

        #endregion

        #region ASYNCHRONOUS METHODS
        private async Task<TReturn> _SqlTryCatchAsync<TReturn>(Func<DbConnection, Task<TReturn>> func, Action<string> exceptionHandler = null)
        {
            DbConnection con = _GetConnection();
            
            try
            {
                if (!_transactionIsActive)
                    await con.OpenAsync().ConfigureAwait(false);

                return await func(con);
            }
            catch (Exception ex)
            {
                if (_transactionIsActive)
                    throw new Exception(ex.Message);
                exceptionHandler(ex.Message);
                return default(TReturn);
            }
            finally
            {
                if (!_transactionIsActive)
                    con.Close();
            }
        }

        #endregion

        #endregion

    }
}
