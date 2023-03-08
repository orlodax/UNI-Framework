namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "documents")]
    public class Document : BaseModel
    {
        [ValueInfo(SQLName = "file")] public byte[] File { get; set; }
        [ValueInfo(SQLName = "name")] public string Name { get; set; }
        [ValueInfo(SQLName = "filename")] public string FileName { get; set; }
        [ValueInfo(SQLName = "refertable")] public string ReferTable { get; set; }
        [ValueInfo(SQLName = "referid")] public int ReferId { get; set; }
    }
}