using System;

namespace UNI.Core.Library
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class ClassInfo : Attribute
    {
        /// <summary>
        /// This is used to map models properties to their tables and columns in the DB
        /// </summary>
        public string SQLName { get; set; } 

        /// <summary>
        /// This is used to retrieve only one type of rows in a common table. For example Hypervisor in table Servers. To work the table must contain the column named classtype
        /// </summary>
        public string ClassType { get; set; } = string.Empty;
    }
}
