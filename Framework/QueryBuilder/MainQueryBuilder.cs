using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    public class MainQueryBuilder : IQueryBuilder
    {
        private IDbProvider _dbProvider;
        private IEntityMapper _entityMapper;

        public MainQueryBuilder(IDbProvider dbProvider, IEntityMapper entityMapper)
        {
            _dbProvider = dbProvider;
            _entityMapper = entityMapper;
        }

        public string CreateInsertStatement<TEntity>(IEnumerable<TEntity> param) 
            where TEntity : class
        {
            string tableName = "";
            string query = "";
            Type entityType = typeof(TEntity);

            List<string> tableList = _GetTableNames();
            tableName = _entityMapper.MapTable(entityType, tableList);

            query = string.Format("INSERT INTO {0} ", tableName);

            List<string> columnList = _GetColumnNames(tableName);
            List<string> columnString = _GetActualColumnNames(entityType, tableName, columnList);

            query += string.Format("({0}) VALUES ", string.Join(", ", columnString));

            query += string.Join(",", _GetEntityValues(param, columnString));

            query = query.TrimEnd(',') + ";";

            return query;
        }

        public string CreateDeleteStatement<TEntity>(TEntity param) where TEntity : class
        {
            string tableName = "";
            string query = "";
            Type entityType = typeof(TEntity);

            List<string> tableList = _GetTableNames();
            tableName = _entityMapper.MapTable(entityType, tableList);

            List<string> columnList = _GetColumnNames(tableName);

            query = string.Format("DELETE FROM {0} WHERE ", tableName);

            PropertyInfo keyProp = entityType.GetProperties().SingleOrDefault(key => 
            {
                KeyAttribute keyAttr = key.GetCustomAttribute<KeyAttribute>(false);

                if (keyAttr != null)
                    return true;

                return false;
            });

            if (keyProp == null)
                throw new Exception("Primary key is not set.");

            string columnName = _entityMapper.MapColumn(keyProp, columnList);

            object value = keyProp.GetValue(param);

            if (value == null)
                throw new Exception("Primary key should not be null.");

            query += string.Format("{0}='{1}';", columnName, value);

            return query;
        }

        public string CreateUpdateStatement<TEntity>(TEntity param) where TEntity : class
        {
            string tableName = "";
            string query = "";
            Type entityType = typeof(TEntity);

            List<string> tableList = _GetTableNames();
            tableName = _entityMapper.MapTable(entityType, tableList);

            query = string.Format("UPDATE {0} SET ", tableName);

            List<string> columnList = _GetColumnNames(tableName);
            List<string> columnString = _GetActualColumnNames(entityType, tableName, columnList);
            KeyValuePair<string, List<string>> keyValuePairs = _GetEntityValue(param, columnString);

            string valueString = string.Join(", ", keyValuePairs.Value);
            query += string.Format("{0} ", valueString);
            query += string.Format("WHERE {0}", keyValuePairs.Key);

            return query;
        }

        private List<string> _GetTableNames()
        {
            List<string> tableNames = new List<string>();

            _dbProvider.LoadDataReader("SHOW TABLES", null, (dr) =>
            {
                tableNames.Add(dr.GetString(0));
            });

            return tableNames;
        }

        private List<string> _GetColumnNames(string tableName)
        {
            List<string> columnNames = new List<string>();

            _dbProvider.LoadDataReader("SHOW COLUMNS FROM " + MySqlHelper.EscapeString(tableName), null, (dr) =>
            {
                columnNames.Add(dr.GetString(0));
            });

            return columnNames;
        }

        private List<string> _GetActualColumnNames(Type entityType, string tableName, List<string> columnList)
        {
            List<string> columnString = new List<string>();

            foreach (PropertyInfo prop in entityType.GetProperties())
            {
                KeyAttribute keyAttr = prop.GetCustomAttribute<KeyAttribute>(false);

                if (keyAttr != null)
                {
                    DbOptionAttribute dbOptionAttr = prop.GetCustomAttribute<DbOptionAttribute>(false);

                    if (dbOptionAttr == null)
                        continue;

                    if (dbOptionAttr.Option == DbGenerateOption.AutoIncrement)
                        continue;
                }

                string columnName = _entityMapper.MapColumn(prop, columnList);

                if(!string.IsNullOrEmpty(columnName))
                    columnString.Add(columnName);
            }

            return columnString;
        }
        private KeyValuePair<string, List<string>> _GetEntityValue<TEntity>(TEntity param, List<string> columnString)
        {
            string key = "";

            List<string> entityValueList = new List<string>();

            foreach (PropertyInfo prop in param.GetType().GetProperties())
            {
                IgnoreAttribute ignoreAttr = prop.GetCustomAttributes<IgnoreAttribute>(false).FirstOrDefault();

                if (ignoreAttr != null)
                    continue;

                object value = prop.GetValue(param);

                if (value == null)
                    continue;

                if (value is DateTime)
                    value = (Convert.ToDateTime(prop.GetValue(param).ToString())).ToString("yyyy-MM-dd HH:mm:ss");
                else
                    value = prop.GetValue(param).ToString();

                KeyAttribute keyAttr = prop.GetCustomAttributes<KeyAttribute>(false).FirstOrDefault();

                if (keyAttr != null)
                    key = string.Format("{0}='{1}'", _entityMapper.MapColumn(prop, columnString), value);
                else
                    entityValueList.Add(string.Format("{0}='{1}'", _entityMapper.MapColumn(prop, columnString), value));
            }

            return new KeyValuePair<string, List<string>>(key, entityValueList);
        }

        private List<string> _GetEntityValues<TEntity>(IEnumerable<TEntity> param, List<string> columnString)
        {
            List<string> entityValueList = new List<string>();

            foreach (TEntity entity in param)
            {
                List<object> objectValues = new List<object>();

                PropertyInfo[] props = entity.GetType().GetProperties();

                foreach (string col in columnString)
                {
                    PropertyInfo prop = props.SingleOrDefault(p => p.Name == col);

                    if (prop == null)
                    {
                        prop = props.SingleOrDefault(p =>
                        {
                            ColumnNameAttribute collNameAttribute = p.GetCustomAttributes<ColumnNameAttribute>(false).FirstOrDefault();
                            if (collNameAttribute == null)
                                return false;

                            return collNameAttribute.ColumnName == col;
                        });
                    }

                    IgnoreAttribute ignoreAttr = prop.GetCustomAttributes<IgnoreAttribute>(false).FirstOrDefault();

                    if (ignoreAttr != null)
                        continue;

                    KeyAttribute keyAttr = prop.GetCustomAttribute<KeyAttribute>(false);

                    if (keyAttr != null)
                    {
                        DbOptionAttribute dbOptionAttr = prop.GetCustomAttribute<DbOptionAttribute>(false);

                        if (dbOptionAttr == null)
                            continue;

                        if (dbOptionAttr.Option == DbGenerateOption.AutoIncrement)
                            continue;
                    }

                    string value = "";

                    if (prop.GetValue(entity) is DateTime)
                        value = (Convert.ToDateTime(prop.GetValue(entity).ToString())).ToString("yyyy-MM-dd HH:mm:ss");
                    else
                        value = prop.GetValue(entity).ToString();

                    objectValues.Add(string.Format("'{0}'", MySqlHelper.EscapeString(value)));
                }

                entityValueList.Add(string.Format("({0})", string.Join(", ", objectValues)));
            }

            return entityValueList;
        }
    }
}
