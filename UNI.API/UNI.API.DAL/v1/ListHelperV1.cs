﻿using MySqlConnector;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using UNI.Core.Library;

namespace UNI.API.DAL.v1
{
    public class ListHelperV1<T>
    {
        private string ConnectionString = string.Empty;
        public ListHelperV1(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public List<T> GetData(string query)
        {
            List<T> list = new List<T>();

            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    try
                    {
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                            list = new ListHelperV1<T>(ConnectionString).CreateList(reader);

                        conn.Close();
                    }
                    catch (MySqlException e)
                    {
                        DbContextV1.OnMySqlError(new DbContextV1.MySqlErrorEventArgs(e));
                        conn.Close();
                    }
                }
            }
            return list;
        }

        public async Task<List<T>> GetDataAsync(string query)
        {
            List<T> list = new List<T>();

            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    try
                    {
                        conn.Open();

                        using (var reader = await cmd.ExecuteReaderAsync())
                            list = new ListHelperV1<T>(ConnectionString).CreateList(reader);

                        conn.Close();
                    }
                    catch (MySqlException e)
                    {
                        DbContextV1.OnMySqlError(new DbContextV1.MySqlErrorEventArgs(e));
                        conn.Close();
                    }
                }
            }
            return list;
        }


        public List<T> CreateList(DbDataReader reader)
        {
            Func<DbDataReader, T> readRow = GetReader(reader);

            var results = new List<T>();

            while (reader.Read())
                results.Add(readRow(reader));

            return results;
        }

        public List<T> SortList(List<T> listToSort, string propertyName, bool ascending)
        {
            // verify that the propertyName is valid
            var propertyNames = typeof(T).GetProperties().ToList().Select(p => p.Name).ToList();
            if (!propertyNames.Contains(propertyName))
                throw new ArgumentOutOfRangeException("There is no property named: " + propertyName);

            var paramExpression = Expression.Parameter(typeof(T), "item");
            var propertyExpression = Expression.Convert(Expression.Property(paramExpression, propertyName), typeof(object));
            var lambdaExpression = Expression.Lambda<Func<T, object>>(propertyExpression, paramExpression);

            if (ascending)
                return listToSort.AsQueryable().OrderBy(lambdaExpression).ToList();
            else
                return listToSort.AsQueryable().OrderByDescending(lambdaExpression).ToList();
        }

        private Func<DbDataReader, T> GetReader(DbDataReader reader)
        {
            Delegate resDelegate;

            List<string> readerColumns = new List<string>();
            for (int index = 0; index < reader.FieldCount; index++)
                readerColumns.Add(reader.GetName(index));

            // a list of LINQ expressions that will be used for each data row
            var statements = new List<Expression>();

            // get the indexer property of DbDataReader
            var indexerProperty = typeof(DbDataReader).GetProperty("Item", new[] { typeof(string) });

            // Parameter expression to create instance of object
            ParameterExpression instanceParam = Expression.Variable(typeof(T));
            ParameterExpression readerParam = Expression.Parameter(typeof(DbDataReader));

            // create and assign new instance of variable
            BinaryExpression createInstance = Expression.Assign(instanceParam, Expression.New(typeof(T)));
            statements.Add(createInstance);

            // loop through each of the properties in T to determine how to populate the new instance properties
            var properties = typeof(T).GetProperties();
            var SQLNames = GetSQLNames(properties);

            foreach (var property in properties)
            {
                string SQLName = SQLNames[property.Name];
                //string SQLName = property.Name;
                if (readerColumns.Contains(SQLName))
                {
                    // get the instance.Property
                    MemberExpression setProperty = Expression.Property(instanceParam, property);

                    // the database column name will be what is in the SQLNames list -- defaults to the property name
                    IndexExpression readValue = Expression.MakeIndex(readerParam, indexerProperty, new[] { Expression.Constant(SQLName) });
                    ConstantExpression nullValue = Expression.Constant(DBNull.Value, typeof(DBNull));
                    BinaryExpression valueNotNull = Expression.NotEqual(readValue, nullValue);
                    if (property.PropertyType.Name.ToLower().Equals("string"))
                    {
                        ConditionalExpression assignProperty = Expression.IfThenElse(valueNotNull, Expression.Assign(setProperty, Expression.Convert(readValue, property.PropertyType)), Expression.Assign(setProperty, Expression.Constant("", typeof(string))));
                        statements.Add(assignProperty);
                    }
                    else
                    {
                        ConditionalExpression assignProperty = Expression.IfThen(valueNotNull, Expression.Assign(setProperty, Expression.Convert(readValue, property.PropertyType)));
                        statements.Add(assignProperty);
                    }
                }
            }
            var returnStatement = instanceParam;
            statements.Add(returnStatement);

            var body = Expression.Block(instanceParam.Type, new[] { instanceParam }, statements.ToArray());
            var lambda = Expression.Lambda<Func<DbDataReader, T>>(body, readerParam);
            resDelegate = lambda.Compile();

            return (Func<DbDataReader, T>)resDelegate;
        }
        private Dictionary<string, string> GetSQLNames(PropertyInfo[] properties)
        {
            var SQLNames = new Dictionary<string, string>();
            foreach (var property in properties)
            {
                string SQLName = property.Name.ToLower();

                var attributes = property.GetCustomAttributes(typeof(ValueInfo), true);
                if (attributes.Length > 0)
                    SQLName = ((ValueInfo)attributes[0]).SQLName;

                SQLNames.Add(property.Name, SQLName);
            }
            return SQLNames;
        }
    }
}



//DOVESSE SERVI

//     foreach (var property in properties)
//            {
//                string SQLName = SQLNames[property.Name];
//                if (readerColumns.Contains(SQLName))
//                {
//                    MemberExpression setProperty = Expression.Property(instanceParam, property);
//    Expression GetValueExpr;

//    IndexExpression readValue = Expression.MakeIndex(readerParam, indexerProperty, new[] { Expression.Constant(SQLName) });
//    ConstantExpression nullValue = Expression.Constant(DBNull.Value, typeof(System.DBNull));
//    BinaryExpression valueNotNull = Expression.NotEqual(readValue, nullValue);

//    MethodInfo GetValueMethod = typeof(David.Utils.Tools).GetMethod("GetValue");
//    MethodInfo genericGetValue = GetValueMethod.MakeGenericMethod(property.PropertyType);
//    GetValueExpr = Expression.Call(null, genericGetValue, new[] { readValue
//});
//                    statements.Add(GetValueExpr);

//                    ConditionalExpression assignProperty = Expression.IfThen(valueNotNull, Expression.Assign(setProperty, GetValueExpr));
//statements.Add(assignProperty);
//                }
//            }
//            var returnStatement = instanceParam;
//statements.Add(returnStatement);

//    namespace David.Utils
//    {
//        public static class Tools
//        {
//            static public T GetValue<T>(object obj)
//            {
//                if (obj.GetType() != typeof(DBNull))
//                {
//                    var t = typeof(T);
//                    return (T)Convert.ChangeType(obj, Nullable.GetUnderlyingType(t) ?? t);
//                }
//                else
//                    return default(T);
//            }
//        }
//    }


