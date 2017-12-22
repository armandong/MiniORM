using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class MySqlService : IDbProvider
    {
        private MySqlConnection _con;
        private string _connectionString;
        private bool _transactionIsActive = false;
        private IEntityMapper _mapper;

        public event EventHandler<ConnectionCreatedEventArgs> ConnectionCreated;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<ExecutedEventArgs> Executed;
        public event EventHandler<LoadDataCompletedEventArgs> LoadDataCompleted;
        public event EventHandler<TransactionStartedEventArgs> TransactionStarted;

        public void OnConnectionCreated(Connection connection)
        {
            ConnectionCreated?.Invoke(this, new ConnectionCreatedEventArgs { ConnectionDetails = connection });
        }

        public void OnConnected()
        {
            Connected?.Invoke(this, new ConnectedEventArgs { ProviderType = DbProviderType.MySql });
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

        public MySqlService(IEntityMapper mapper)
        {
            _mapper = mapper;
        }

        #region PUBLIC

        #region SYNCHRONOUS METHODS
        public IDbProvider CreateConnectionMySql(Connection connection)
        {
            _connectionString = string.Format("server={0};database={1};uid={2};password={3}",
                    connection.Host, connection.Database, connection.User, connection.Password);

            _con = new MySqlConnection(_connectionString);

            OnConnectionCreated(connection);

            Task.Run(() => TestConnectionAsync());

            return this;
        }

        public int? Execute(string query, object sqlParam = null, CommandType cmdType = CommandType.Text)
        {
            return _MySqlTryCatch(con =>
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = query;
                    cmd.CommandType = cmdType;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam));

                    cmd.ExecuteNonQueryAsync();

                    ExecuteType executeType = MiniOrmHelpers.CheckExecuteType(query);

                    int lastInsertId = Convert.ToInt32(cmd.LastInsertedId);
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
            return _MySqlTryCatch((con) =>
            {
                List<TEntity> entityListTemp = new List<TEntity>();
                using (MySqlCommand cmd = new MySqlCommand(query, _GetConnection()))
                {
                    cmd.Connection = con;
                    cmd.CommandText = query;
                    cmd.CommandType = cmdType;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam));

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
            return _MySqlTryCatch<object>((con) =>
            {
                using (MySqlCommand cmd = new MySqlCommand(query, _GetConnection()))
                {
                    cmd.Connection = con;
                    cmd.CommandText = query;
                    cmd.CommandType = cmdType;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam));

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

        public object LoadSingleItem(string query, MySqlParameter[] sqlParam = null,
                                CommandType cmdType = CommandType.Text)
        {
            return _MySqlTryCatch((con) =>
            {
                using (MySqlCommand cmd = new MySqlCommand(query, _GetConnection()))
                {
                    cmd.Connection = con;
                    cmd.CommandText = query;
                    cmd.CommandType = cmdType;

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
            try
            {
                using (MySqlConnection con = _GetConnection())
                {
                    await con.OpenAsync().ConfigureAwait(false);
                    callback?.Invoke(null);
                }
            }
            catch (MySqlException ex)
            {
                callback?.Invoke(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                callback?.Invoke(ex.Message);
                return false;
            }

            OnConnected();
            return true;
        }

        public async Task<int?> ExecuteAsync(string query, object sqlParam = null, CommandType cmdType = CommandType.Text)
        {
            return await _MySqlTryCatchAsync(async (con) =>
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = query;
                    cmd.CommandType = cmdType;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam));

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    ExecuteType executeType = MiniOrmHelpers.CheckExecuteType(query);

                    int lastInsertId = Convert.ToInt32(cmd.LastInsertedId);
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

            return await _MySqlTryCatchAsync(async (con) =>
            {
                List<TEntity> entityListTemp = new List<TEntity>();
                using (MySqlCommand cmd = new MySqlCommand(query, _GetConnection()))
                {
                    cmd.Connection = con;
                    cmd.CommandText = query;
                    cmd.CommandType = cmdType;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam));

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

            return await _MySqlTryCatchAsync(async (con) =>
            {
                using (MySqlCommand cmd = new MySqlCommand(query, _GetConnection()))
                {
                    cmd.Connection = con;
                    cmd.CommandText = query;
                    cmd.CommandType = cmdType;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(_SetParams(sqlParam));

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

        public async Task<object> LoadSingleItemAsync(string query, MySqlParameter[] sqlParam = null,
                                CommandType cmdType = CommandType.Text)
        {
            return await _MySqlTryCatchAsync(async (con) =>
            {
                using (MySqlCommand cmd = new MySqlCommand(query, _GetConnection()))
                {
                    cmd.Connection = con;
                    cmd.CommandText = query;
                    cmd.CommandType = cmdType;

                    if (sqlParam != null)
                        cmd.Parameters.AddRange(sqlParam);

                    return await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                }
            });
        }

        public async Task<bool> TransactionAsync(Action action)
        {
            MySqlTransaction trans = null;
            try
            {
                using (MySqlConnection con = _GetConnection())
                {
                    await con.OpenAsync().ConfigureAwait(false);
                    trans = await con.BeginTransactionAsync().ConfigureAwait(false);

                    OnTransactionStarted();

                    _transactionIsActive = true;
                    action?.Invoke();

                    trans.Commit();

                    con.Disposed += (s, e) => { _transactionIsActive = false; };
                }
            }
            catch (Exception ex)
            {
                if (ex is MySqlException)
                {
                    MySqlException mysqlException = ex as MySqlException;
                }

                trans.Rollback();
                return false;
            }

            return true;
        }

        #endregion

        #endregion

        #region PRIVATE

        #region SYNCHRONOUS METHODS
        private MySqlConnection _GetConnection()
        {
            if (_transactionIsActive == false)
            {
                _con = new MySqlConnection(_connectionString);
            }

            return _con;
        }

        private TReturn _MySqlTryCatch<TReturn>(Func<MySqlConnection, TReturn> func, Action<string> exceptionHandler = null)
        {
            MySqlConnection con = _GetConnection();

            try
            {
                if (!_transactionIsActive)
                    con.Open();

                return func(con);
            }
            catch (MySqlException ex)
            {
                if (_transactionIsActive)
                    throw new Exception(ex.Message);

                exceptionHandler(ex.Message);
                return default(TReturn);
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
        private MySqlParameter[] _SetParams(object param)
        {
            PropertyInfo[] props = param.GetType().GetProperties() as PropertyInfo[];
            List<MySqlParameter> sqlParams = new List<MySqlParameter>();

            foreach (PropertyInfo prop in props)
            {
                string propName = prop.Name;
                sqlParams.Add(new MySqlParameter("@" + propName, prop.GetValue(param, null)));
            }

            return sqlParams.ToArray();
        }


        private MySqlParameter[] _SetParamsExecute(object param, string query = "")
        {
            if (!(param is Array))
                return _SetParams(param);

            List<MySqlParameter> sqlParams = new List<MySqlParameter>();

            foreach (dynamic item in (dynamic[])param)
            {
                foreach (PropertyInfo prop in item.GetType().GetProperties() as PropertyInfo[])
                {

                }
            }

            throw new NotImplementedException();
        }

        #endregion

        #region ASYNCHRONOUS METHODS
        private async Task<TReturn> _MySqlTryCatchAsync<TReturn>(Func<MySqlConnection, Task<TReturn>> func, Action<string> exceptionHandler = null)
        {
            MySqlConnection con = _GetConnection();

            try
            {
                if (!_transactionIsActive)
                    await con.OpenAsync().ConfigureAwait(false);

                return await func(con);
            }
            catch (MySqlException ex)
            {
                if (_transactionIsActive)
                    throw new Exception(ex.Message);
                exceptionHandler(ex.Message);
                return default(TReturn);
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
                    await con.CloseAsync();
            }
        }

        #endregion

        #endregion

       
    }
}
