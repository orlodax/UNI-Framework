using System.Collections;
using System.Diagnostics;
using System.Reflection;
using UNI.API.Contracts.Models;
using UNI.Core.Library;
using UNI.Core.Library.AttributesMetadata;
namespace UNI.API.Tests;

[TestClass]
public class VariousTests
{
    [TestMethod]
    public void GroupBaseModelMetadata()
    {
        User obj = new();
        IEnumerable<DataAttributes> dataAttributes = obj.Metadata.DataAttributes.Values.Distinct();
        PropertyInfo[] properties = typeof(User).GetProperties();


        IEnumerable<IGrouping<string, KeyValuePair<string, DataAttributes>>> groupsOfWriteTablesAndProperties = obj.Metadata.DataAttributes.Where(d => !string.IsNullOrWhiteSpace(d.Value.WriteTable)).GroupBy(d => d.Value.WriteTable);

        foreach (IGrouping<string, KeyValuePair<string, DataAttributes>> group in groupsOfWriteTablesAndProperties)
        {
            Debug.Print($"GROUP KEY {group.Key}");

            foreach (var b in group)
            {
                Debug.Print($"  key {b.Key}");
                Debug.Print($"  value {b.Value}");
            }
        }
    }

    //[TestMethod]
    //public void IListTypePopulation()
    //{
    //    IList values = new List<BaseModel>();
    //    values = PopulateList();
    //    Assert.IsTrue( values.Count == 2 );
    //}

    //private static List<User> PopulateList()
    //{
    //    return new List<User>()
    //    {
    //        new User()
    //        {
    //            ID = 1,
    //            FirstName="test1"
    //        },
    //        new User()
    //        {
    //            ID = 2,
    //            FirstName="test2"
    //        },
    //    };
    //}
}