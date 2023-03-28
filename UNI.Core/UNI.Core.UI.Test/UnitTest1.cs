using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using UNI.Core.Library.GenericModels;

namespace UNI.Core.UI.Test
{
    [TestClass]
    public class Tests
    {

        [TestMethod]
        public void Test1()
        {

            // var vm = new ViewModelDerivato();
            // vm.VBBase.MetodoBase();

            //Type classType = typeof(List<>);
            //Type[] typeParams = new Type[] { typeof(BaseCustomer) };
            //Type constructedType = classType.MakeGenericType(typeParams);

            //if (constructedType.Equals(typeof(List<Customer>)))
            //    Assert.Pass();
            //else
            //    Assert.Fail();
        }

        [TestMethod]
        public void Test2()
        {
            //var name = nameof(BaseCustomer);

            //if (name == "Customer")
            //    Assert.Pass();
            //else
            //    Assert.Fail();
        }

        [TestMethod]
        public void TestAboutBaseTypeSearchingForBaseModel()
        {
            //var bob = UtilityMethods.FindAllParentsTypes(typeof(Customer));

            //bool isBaseModel = typeof(Customer).IsSubclassOf(typeof(BaseModel));
            //Assert.True(isBaseModel);
        }

        [TestMethod]
        public void FilterPropertyInfosViaList()
        {
            var filterPropertyTypes = new List<Type>
            {
                typeof(string),
                typeof(DateTime),
                typeof(DateTimeOffset)
            };
            var properties = typeof(BaseCustomer).GetProperties().Where(p => filterPropertyTypes.Any(f => f == p.PropertyType)).ToArray();

        }


        [TestMethod]
        public void LoadResourcesFromEnumValues()
        {
            var timeRangesCount = Enum.GetNames(typeof(EnTimeRanges)).Length;
            List<string> timeRanges = new List<string>();
            for (int i = 0; i < timeRangesCount; i++)
                timeRanges.Add(Enum.GetName(typeof(EnTimeRanges), i).ToLower());

        }

        enum EnTimeRanges
        {
            Day = 0,
            Month = 1,
            Year = 2
        }

    }
}