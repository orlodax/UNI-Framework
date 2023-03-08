namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "images")]
    public class UniImage : BaseModel
    {
        [ValueInfo(SQLName = "modelname")] public string ModelName { get; set; }
        [ValueInfo(SQLName = "propertyname")] public string PropertyName { get; set; }
        [ValueInfo(SQLName = "idbasemodel")] public int IdBaseModel { get; set; }
        [ValueInfo(SQLName = "source")] public byte[] Source { get; set; }
    }
}
