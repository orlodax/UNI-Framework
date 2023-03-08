namespace UNI.Core.Library
{
    /// <summary>
    /// Used by DatabaseV1.cs && DatabaseV2.cs in UNI.API.DAL
    /// </summary>
    public interface IMultipleRelation
    {
        int IdRef { get; set; }
        string TableRef { get; set; }

        /// <summary>
        /// The object to which the table points
        /// </summary>
        BaseModel Referee { get; set; }
    }
}
