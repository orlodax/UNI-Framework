﻿using MySqlConnector;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using UNI.Core.Library;

namespace UNI.API.DAL.v2;

public class ListHelperV2<T> where T : BaseModel
{
    private readonly string connectionString = string.Empty;
    public ListHelperV2(string connectionString)
    {
        this.connectionString = connectionString;
    }
    public object GetGenericInstance(string connectionString, Type genericArg)
    {
        return Activator.CreateInstance(typeof(ListHelperV2<>).MakeGenericType(genericArg), new[] { connectionString });
    }

    public async Task<List<T>> GetData(string query)
    {
        List<T> list = new();

        using MySqlConnection conn = new(connectionString);
        using MySqlCommand cmd = new(query, conn);
        cmd.CommandTimeout = 120;
        try
        {
            conn.Open();

            using (var reader = await cmd.ExecuteReaderAsync())
                list = new ListHelperV2<T>(connectionString).CreateList(reader);

            conn.Close();
        }
        catch (MySqlException)
        {
            conn.Close();
        }
        
        return list;
    }

    private List<T> CreateList(DbDataReader reader)
    {
        Func<DbDataReader, T> readRow = GetReader(reader);

        List<T> results = new();

        while (reader.Read())
            results.Add(readRow(reader));

        return results;
    }

    //private static List<T> SortList(List<T> listToSort, string propertyName, bool ascending)
    //{
    //    // verify that the propertyName is valid
    //    var propertyNames = typeof(T).GetProperties().ToList().Select(p => p.Name).ToList();
    //    if (!propertyNames.Contains(propertyName))
    //        throw new ArgumentOutOfRangeException("There is no property named: " + propertyName);

    //    var paramExpression = Expression.Parameter(typeof(T), "item");
    //    var propertyExpression = Expression.Convert(Expression.Property(paramExpression, propertyName), typeof(object));
    //    var lambdaExpression = Expression.Lambda<Func<T, object>>(propertyExpression, paramExpression);

    //    if (ascending)
    //        return listToSort.AsQueryable().OrderBy(lambdaExpression).ToList();
    //    else
    //        return listToSort.AsQueryable().OrderByDescending(lambdaExpression).ToList();
    //}

    private static Func<DbDataReader, T> GetReader(DbDataReader reader)
    {
        Delegate resDelegate;

        List<string> readerColumns = new();

        for (int index = 0; index < reader.FieldCount; index++)
            readerColumns.Add(reader.GetName(index));

        // a list of LINQ expressions that will be used for each data row
        List<Expression> statements = new();

        // get the indexer property of DbDataReader
        PropertyInfo? indexerProperty = typeof(DbDataReader).GetProperty("Item", new[] { typeof(string) });

        // Parameter expression to create instance of object
        ParameterExpression instanceParam = Expression.Variable(typeof(T));
        ParameterExpression readerParam = Expression.Parameter(typeof(DbDataReader));

        // create and assign new instance of variable
        BinaryExpression createInstance = Expression.Assign(instanceParam, Expression.New(typeof(T)));
        statements.Add(createInstance);

        // loop through each of the properties in T to determine how to populate the new instance properties
        PropertyInfo[]? properties = typeof(T).GetProperties();
        Dictionary<string, string>? SQLNames = ListHelperV2<T>.GetSQLNames(properties);

        foreach (PropertyInfo? property in properties)
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

    //TODO fare che sqlname è facoltativo (default = property.name.ToLower() ) 
    private static Dictionary<string, string> GetSQLNames(PropertyInfo[] properties)
    {
        Dictionary<string, string> SQLNames = new();
        foreach (PropertyInfo? property in properties)
        {
            string SQLName = property.Name.ToLower();

            object[]? attributes = property.GetCustomAttributes(typeof(ValueInfo), true);
            if (attributes.Length > 0)
                SQLName = ((ValueInfo)attributes[0]).SQLName;

            SQLNames.Add(property.Name, SQLName);
        }
        return SQLNames;
    }
}