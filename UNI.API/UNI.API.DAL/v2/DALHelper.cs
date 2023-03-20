using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UNI.Core.Library;

namespace UNI.API.DAL.v2;

public class DALHelper
{
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