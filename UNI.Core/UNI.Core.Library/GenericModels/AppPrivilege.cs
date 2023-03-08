namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "userprivileges")]
    public class AppPrivilege : BaseModel
    {
        [ValueInfo(SQLName = "name")] public string Name { get; set; }
        [ValueInfo(SQLName = "privilege")] public string Privilege { get; set; }
        [ValueInfo(SQLName = "type")] public string Type { get; set; }
        [ValueInfo(SQLName = "iduser")] public int IdAppUser { get; set; }
    }
}