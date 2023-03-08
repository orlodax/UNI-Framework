using Newtonsoft.Json.Linq;
using System.Collections;
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
}
