namespace UNI.Core.Library.AttributesMetadata
{
    /// <summary>
    /// Describes the relation between a BaseModel and its table(s) or view
    /// </summary>
    public enum EnBaseModelTypes
    {  
        /// <summary>
        /// Normal implementation: to one basemodel corresponds one table in the db
        /// </summary>
        OneTableOneBaseModel,

        /// <summary>
        /// The basemodel A extends basemodel B and comes from a view that extends table A 
        /// </summary>
        ExtendingBaseModel,

        /// <summary>
        /// The basemodel is based on a view directly, without extending other basemodels, (the view can refer multiple tables)
        /// </summary>
        ViewOnlyBaseModel
    }
}
