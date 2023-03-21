using Microsoft.Extensions.Logging;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using UNI.Core.Library;

namespace UNI.API.DAL.v2;

public class DALHelper
{
    public static DataTable ExecuteReadQuery(string query, string connectionString)
    {
        DataTable dataset = new();

        using MySqlConnection conn = new(connectionString);
        using MySqlCommand cmd = new(query, conn);

        try
        {
            conn.Open();
            DbDataReader reader;
            reader = cmd.ExecuteReader();

            dataset.Load(reader);
            conn.Close();
        }
        catch (MySqlException)
        {
            conn.Close();
        }

        return dataset;
    }

    /// <summary>
    /// Can perform all writing queries; queryParameters is for byte blobs only though
    /// </summary>
    /// <param name="query"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    public static async Task<int> SetData(string query, string connectionString, ILogger logger, Dictionary<string, byte[]>? queryParameters = null)
    {
        int lastId;

        using MySqlConnection conn = new(connectionString);
        using MySqlCommand cmd = new(query, conn);

        if (queryParameters != null)
            foreach (string key in queryParameters.Keys)
                if (queryParameters.TryGetValue(key, out byte[]? value))
                    if (value != null)
                        cmd.Parameters.Add(new MySqlParameter($"@{key}", MySqlDbType.LongBlob, value.Length) { Value = value });

        conn.Open();

        try
        {
            await cmd.ExecuteNonQueryAsync();
            lastId = Convert.ToInt32(cmd.LastInsertedId);
        }
        catch (MySqlException e)
        {
            LogError(logger, e);
            lastId = 0;
        }

        conn.Close();

        return lastId;
    }

    /// <summary>
    /// Can perform writing queries with safe sql parameters
    /// </summary>
    /// <param name="query"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    public static async Task SetDataParametrized(string query, MySqlParameter[] queryParameters, string connectionString, ILogger logger)
    {
        using MySqlConnection conn = new(connectionString);
        using MySqlCommand cmd = new(query, conn);

        cmd.Parameters.AddRange(queryParameters);

        conn.Open();

        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (MySqlException e)
        {
            LogError(logger, e);
        }

        conn.Close();
    }

    private static void LogError(ILogger logger, MySqlException e)
    {
        logger.Log(LogLevel.Error, $"{logger.GetType().GetGenericArguments()[0]}: Error message: '{e.Message}'. Inner exception: '{e.InnerException?.Message}'");
    }

    public static List<Type> GetMyPath(List<Type> fatherPath, Type propertyType)
    {
        List<Type> mypath = new();
        foreach (var type in fatherPath)
            mypath.Add(type);

        mypath.Add(propertyType);

        return mypath;
    }

    public static List<BaseModel> GetTypeListBaseModel(IList typeList, Type propertyType)
    {
        List<BaseModel> typeListBaseModel = new();

        foreach (var entry in typeList)
            if (entry is JObject jobject)
                if (jobject.ToObject(propertyType) is BaseModel bm)
                    typeListBaseModel.Add(bm);

        return typeListBaseModel;
    }

    internal static string? GetSelectObjectsNoFillQuery(Type baseModelType,
                                                      string? tableAttritbute = null,
                                                      Dictionary<string, List<int>>? idsToMatch = null,
                                                      Dictionary<string, string>? valuesToMatch = null)
    {
        var classInfo = (ClassInfo?)baseModelType.GetCustomAttribute(typeof(ClassInfo));
        if (classInfo == null)
            return null;

        tableAttritbute ??= classInfo.SQLName;

        string query = "SELECT * FROM " + tableAttritbute;

        if (idsToMatch != null && idsToMatch.Count > 0)
        {
            foreach (KeyValuePair<string, List<int>> entry in idsToMatch)
            {
                if (entry.Value.Count > 0)
                {
                    query += " WHERE";
                    foreach (var id in entry.Value)
                        query += $" id{entry.Key} = {id} OR";

                    query = query.Remove(query.Length - 3, 3);
                }
            }

            if (!string.IsNullOrWhiteSpace(classInfo.ClassType))
                query += $" AND classtype = '{classInfo.ClassType}'";
        }
        else if (valuesToMatch != null && valuesToMatch.Count > 0)
        {
            foreach (KeyValuePair<string, string> entry in valuesToMatch)
                if (!string.IsNullOrWhiteSpace(entry.Value))
                    query += $" WHERE {entry.Key} LIKE '%{entry.Value}%' ";

            if (!string.IsNullOrWhiteSpace(classInfo.ClassType))
                query += $" AND classtype = '{classInfo.ClassType}'";
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(classInfo.ClassType))
                query += $" WHERE classtype = '{classInfo.ClassType}'";
        }

        return query;
    }

    internal static async Task<object> InvokeAsyncList(object reflectedClass, MethodInfo reflectedMethod, object[] args)
    {
        var task = (Task)reflectedMethod.Invoke(reflectedClass, args);
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        return resultProperty.GetValue(task);
    }
}