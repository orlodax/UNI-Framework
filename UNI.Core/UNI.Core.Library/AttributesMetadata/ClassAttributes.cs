namespace UNI.Core.Library.AttributesMetadata
{
    /// <summary>
    /// Holds all "attributes" previously contained in ClassInfo
    /// </summary>
    public class ClassAttributes
    {
        /// <summary>
        /// This is used to map models properties to their tables and columns in the DB
        /// </summary>
        public string SQLName { get; set; }

        /// <summary>
        /// This is used to retrieve only one type of rows in a common table. For example Hypervisor in table Servers. To work the table must contain the column named classtype
        /// </summary>
        public string ClassType { get; set; } = string.Empty;

        /// <summary>
        /// In case BaseModelType is ViewOnlyBaseModel, we need to know that writetables different from this will need to perform the insert with a where clause = $"id{typename/tablename}"
        /// </summary>
        public string MasterTable { get; set; }

        public EnBaseModelTypes BaseModelType { get; set; } = EnBaseModelTypes.OneTableOneBaseModel;
    }
}
