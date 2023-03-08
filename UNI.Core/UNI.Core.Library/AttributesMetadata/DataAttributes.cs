namespace UNI.Core.Library.AttributesMetadata
{
    /// <summary>
    /// Holds all "attributes" consumed by the API/backend (previously contained in ValueInfo and SqlFieldInfo)
    /// </summary>
    public class DataAttributes
    {
        /// <summary>
        /// Map models properties to their tables and columns in the DB
        /// </summary>
        public string SQLName { get; set; }

        /// <summary>
        /// When the field comes from a table different than the name of the model (i.e. it's read through a view)
        /// </summary>
        public string WriteTable { get; set; }

        /// <summary>
        /// Map models properties to their tables and columns in the DB
        /// </summary>
        public string ManyToManySQLName { get; set; }

        /// <summary>
        /// Map models properties to their tables and columns in the DB
        /// </summary>
        public string LinkTableSQLName { get; set; }

        /// <summary>
        /// TODO change to list<string>
        /// </summary>
        public string ColumnReference1Name { get; set; }

        /// <summary>
        /// TODO change to list<string>
        /// </summary>
        public string ColumnReference2Name { get; set; }

        /// <summary>
        /// TODO change from column 1 - 2
        /// </summary>
        public string[] ColumnReferenceNames { get; set; }

        /// <summary>
        /// TODO: davero? used for one to one different behavour to generate showboxOnetoOne
        /// </summary>
        public bool IsOneToOne { get; set; }

        /// <summary>
        /// False = Must not be inserted in update or insert query
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// False = it will be inserted in query but user cannot edit it
        /// </summary>
        public bool IsUserReadOnly { get; set; }

        /// <summary>
        /// Marks the property as main numeration of a table, used for rapid creation
        /// </summary>
        public bool IsMainNumeration { get; set; }

        /// <summary>
        /// Marks the property as default data filter
        /// </summary>
        public bool IsDefaultDataFilter { get; set; }

        /// <summary>
        /// Set a dependency to a parent's property, used in NewItem logic
        /// </summary>
        public string ParentPropertyDependendency { get; set; }

        /// <summary>
        /// TODO refactor using names in DB
        /// This is used set a complex query executed directly by the api to retrieve a value. In this way in case of massive data would be the sql engine to do 
        /// the heavy work without passing large amount of data. Example: Field Quantity in product is the sum of all product movements that potentially could be massive
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// TODO, se confermate queste ultime due, cambiare a lista
        /// Contains LIST of properties used for the creation of the where clause. If are needed multiple properties they must be separated by commas and in order
        /// </summary>
        public string PropertyWhereValues { get; set; }
    }
}
