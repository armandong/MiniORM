using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MiniORM
{
    public class EntityMapper : IEntityMapper
    {
        public TEntity Map<TEntity>(IDataReader reader, List<string> columns) where TEntity : class, new()
        {
            TEntity entity = new TEntity();

            foreach (PropertyInfo prop in typeof(TEntity).GetProperties())
            {
                string value = MapColumn(prop, columns);

                if (string.IsNullOrEmpty(value))
                    continue;

                prop.SetValue(entity, Convert.ChangeType(reader[value].ToString(), prop.PropertyType), null);
            }

            return entity;
        }

        public string MapColumn(PropertyInfo prop, List<string> columns)
        {
            IgnoreAttribute ignoreAttr = prop.GetCustomAttributes<IgnoreAttribute>(false).FirstOrDefault();

            if (ignoreAttr != null)
                return null;
                
            ColumnNameAttribute colNameAttr = null;
            string columnName = "";

            if (!columns.Any(x => x == prop.Name))
            {
                colNameAttr = prop.GetCustomAttributes<ColumnNameAttribute>(false).FirstOrDefault();

                if (colNameAttr == null)
                    throw new Exception("Column name doesn't exist.");

                if (!columns.Any(x => x == colNameAttr.ColumnName))
                    throw new Exception("Column name doesn't exist.");

                columnName = colNameAttr.ColumnName;
            }
            else
                columnName = prop.Name;

            return columnName;
        }

        public string MapTable(Type entityType, List<string> tableList)
        {
            string tableName = "";

            if (tableList.Any(t => t == entityType.Name))
                tableName = entityType.Name;
            else
            {
                TableAttribute tableAttr = entityType.GetCustomAttributes<TableAttribute>(false).FirstOrDefault();

                if (tableAttr == null)
                    throw new Exception("Table name doesn't exist.");

                if (!tableList.Any(t => t == tableAttr.TableName))
                    throw new Exception("Table name doesn't exist.");

                tableName = tableAttr.TableName;
            }

            return tableName;
        }
    }
}
