using UNI.Core.Library.GenericModels.Mapping;

namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "productsserials")]
    public class ProductSerial : BaseModel
    {
        [ValueInfo(SQLName = "idproduct", ParentPropertyDependendency = "IdProduct")] public int IdProduct { get; set; }
        [ValueInfo(SQLName = "serialcode", IsDisplayProperty = true)] public string SerialCode { get; set; }
        [ValueInfo(IsReadOnly = true)] public BaseProduct Product { get; set; }
        // TODO create query family in database, use naming convention entity relations/table names etc
        [ValueInfo(IsReadOnly = true)][SqlFieldInfo(Query = " select if ((select sum(somma) from ( SELECT IF(quantity > 0, 1, -1) as somma FROM viewproductsmovementsserials where idMtm = {0})src)>0,1,0)", PropertyWhereValues = "ID")] public double Quantity { get; set; }

    }
}