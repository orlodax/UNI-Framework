using UNI.Core.Library;
using UNI.Core.Library.Mapping;

namespace Uni.Colmar.Library
{
    [ClassInfo(SQLName = "products")]
    public class ServiceProduct : BaseModel
    {
        [ValueInfo(SQLName = "maincode")] public string MainCode { get; set; }
        [ValueInfo(SQLName = "um")] public string Um { get; set; }
        [ValueInfo(SQLName = "description", IsDisplayProperty = true)][RenderInfo(NewLine = true)] public string Description { get; set; }
        [ValueInfo(SQLName = "notes")] public string Notes { get; set; }
        [ValueInfo(SQLName = "defaultpurchaseprice")][RenderInfo(Group = "PriceGroup")] public double DefaultPurchasePrice { get; set; }
        [ValueInfo(SQLName = "defaultsellprice")][RenderInfo(Group = "PriceGroup")] public double DefaultSellPrice { get; set; }
        [ValueInfo(IsReadOnly = true)][RenderInfo(Group = "PriceGroup")] public double DefaultGrossPrice { get; set; }
        [ValueInfo(SQLName = "defaultvat")][RenderInfo(Group = "PriceGroup")] public double DefaultVat { get; set; }
        [ValueInfo(SQLName = "isservice")] public bool IsService { get; set; }

        public ServiceProduct()
        {
            DefaultGrossPrice = DefaultSellPrice * (1 + (DefaultVat / 100));
        }

    }
}