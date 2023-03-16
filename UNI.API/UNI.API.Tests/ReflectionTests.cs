using System.Diagnostics;
using System.Reflection;
using UNI.API.DAL.v2;
using UNI.Core.Library.GenericModels;
using UNI.Core.Library;

namespace UNI.API.Tests
{
    [TestClass]
    public class ReflectionTests
    {
        [TestMethod]
        public void MakeGenericMethodOverwritesTheGenericArgumentOfItsContainingClass()
        {
            var listHelper = new ListHelperV2<Employee>("connectionString");
            var genericListHelper = listHelper.GetGenericInstance("connectionString", typeof(Document));
            var getDataMethod = genericListHelper.GetType().GetMethod(nameof(ListHelperV2<Employee>.GetData));

            //var method = listHelper.GetType().GetMethod(nameof(ListHelperV2<BaseModel>.TestMethod));
            //var genericMethod = method?.MakeGenericMethod(typeof(Document));
            //var items = genericMethod?.Invoke(listHelper, null);
        }


        [TestMethod]
        public void ShouldWorkInvokeMethodWithReflection()
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
        public void ShouldNotWorkInvokeMethodWithReflectionPassingWrongParameterType()
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
        public bool GenericMethodToReflectUpon(string arg1, T arg2)
        {
            Debug.Print($"I was called with type {typeof(T).Name} and my args were {arg1} and {arg2}");

            return true;
        }
    }
}
