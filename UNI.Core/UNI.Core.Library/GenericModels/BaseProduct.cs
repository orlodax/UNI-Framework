using UNI.Core.Library.GenericModels.Mapping;
using UNI.Core.Library.Mapping;

namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "products")]
    public class BaseProduct : BaseModel
    {
        [ValueInfo(SQLName = "maincode")] public string MainCode { get; set; }
        [ValueInfo(SQLName = "barcode")] public string Barcode { get; set; }
        [ValueInfo(SQLName = "eancode")] public string EanCode { get; set; }
        [ValueInfo(SQLName = "um")] public string Um { get; set; }
        [ValueInfo(SQLName = "weight")] public double Weight { get; set; }
        [ValueInfo(SQLName = "volume")] public double Volume { get; set; }
        [ValueInfo(SQLName = "description", IsDisplayProperty = true)][RenderInfo(NewLine = true)] public string Description { get; set; }
        [ValueInfo(SQLName = "notes")] public string Notes { get; set; }
        [ValueInfo(SQLName = "defaultpurchaseprice")][RenderInfo(Group = "PriceGroup")] public double DefaultPurchasePrice { get; set; }
        [ValueInfo(SQLName = "defaultsellprice")][RenderInfo(Group = "PriceGroup")] public double DefaultSellPrice { get; set; }
        [ValueInfo(IsReadOnly = true)][RenderInfo(Group = "PriceGroup")] public double DefaultGrossPrice { get; set; }
        [ValueInfo(SQLName = "defaultvat")][RenderInfo(Group = "PriceGroup")] public double DefaultVat { get; set; }
        [ValueInfo(SQLName = "isservice")] public bool IsService { get; set; }
        [ValueInfo()][RenderInfo(PageGroup = "Movements")] public UniDataSet<BaseProductMovement> ProductMovements { get; set; }
        [ValueInfo()][RenderInfo(PageGroup = "Serials")] public UniDataSet<ProductSerial> ProductSerials { get; set; }

        //TODO create query family in database with naming conventions
        [ValueInfo(IsReadOnly = true)]
        [SqlFieldInfo(Query = "SELECT SUM(quantity) FROM productsmovements WHERE idproduct = {0};",
                      PropertyWhereValues = "ID")]
        public double Quantity { get; set; }

        public BaseProduct()
        {
            DefaultGrossPrice = DefaultSellPrice * (1 + (DefaultVat / 100));
        }
    }
}