using System;

namespace UNI.Core.Library.GenericModels.Mapping
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class SqlFieldInfo : Attribute
    {
        // TODO refactor using names in DB
        public string Query { get; set; }               // This is used set a complex query executed directly by the api to retrieve a value. In this way in case of massive data would be the sql engine to do 
                                                        // the heavy work without passing large amount of data. Example: Field Quantity in product is the sum of all product movements that potentially could be massive

        public string PropertyWhereValues { get; set; } //Contains the list of properties used for the creation of the where clause. If are needed multiple properties they must be separated by commas and in order
    }
}