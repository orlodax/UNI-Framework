using System.Diagnostics;
using System.Reflection;
using UNI.API.Contracts.Models;
using UNI.Core.Library.AttributesMetadata;

namespace UNI.API.Tests
{
    [TestClass]
    public class ReflectionTests
    {
        [TestMethod]
        public async Task GroupBaseModelMetadata()
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

        [TestMethod]
        public async Task ShouldWorkInvokeMethodWithReflection()
        {
            Type typeArg = typeof(int);
            Type genericClass = typeof(ClassToReflectUpon<>);
            Type constructedClass = genericClass.MakeGenericType(typeArg);
            var created = Activator.CreateInstance(constructedClass);
            MethodInfo? genericMethod = created?.GetType().GetMethod("GenericMethodToReflectUpon");
            object[] parameters = { "ciao", 42 };

            genericMethod?.Invoke(created, parameters);
        }

        [TestMethod]
        public async Task ShouldNotWorkInvokeMethodWithReflectionPassingWrongParameterType()
        {
            Type typeArg = typeof(int);
            Type genericClass = typeof(ClassToReflectUpon<>);
            Type constructedClass = genericClass.MakeGenericType(typeArg);
            var created = Activator.CreateInstance(constructedClass);
            MethodInfo? genericMethod = created?.GetType().GetMethod("GenericMethodToReflectUpon");
            object[] parameters = { "ciao", "ciaone" };

            genericMethod?.Invoke(created, parameters);
        }

    }

    public class ClassToReflectUpon<T>
    {
        public async Task<bool> GenericMethodToReflectUpon(string arg1, T arg2)
        {
            Debug.Print($"I was called with type {typeof(T).Name} and my args were {arg1} and {arg2}");

            return true;
        }
    }
}
