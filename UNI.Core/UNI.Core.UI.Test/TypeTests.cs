using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UNI.Core.UI.Test
{
    [TestClass]
    public class TypeTests
    {
        [TestMethod]
        public void GetTitle()
        {
            var sender = new ListGridVMCommesse();

            Type baseType = sender.GetType().BaseType;

            Assert.IsTrue(baseType.GetGenericArguments().Any());
        }
    }
}
