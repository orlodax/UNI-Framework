using System.Collections.Generic;
using UNI.Core.Library.Converters;
using UNI.Core.Library.Mapping;

namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "productsmovements")]
    public class InboundDDTRow : BaseProductMovement
    {
        [ValueInfo(SQLName = "unitnetprice")][RenderInfo(Converter = typeof(CurrencyConverter))] public double UnitNetPrice { get; set; }
        [ValueInfo(SQLName = "unitpurchasenetprice")][RenderInfo(Converter = typeof(CurrencyConverter))] public double UnitPurchaseNetPrice { get; set; }
        [ValueInfo(SQLName = "vatpercentage")][RenderInfo(Converter = typeof(PercentageConverter))] public double VatPercentage { get; set; }
        [ValueInfo(IsReadOnly = true)][RenderInfo(Converter = typeof(CurrencyConverter))] public double UnitVat { get; set; }
        [ValueInfo(IsReadOnly = true)][RenderInfo(Converter = typeof(CurrencyConverter))] public double UnitGrossPrice { get; set; }
        [ValueInfo(IsReadOnly = true)][RenderInfo(Converter = typeof(CurrencyConverter))] public double GrossPrice { get; set; }

        [ValueInfo(ManyToManySQLName = "viewproductsserialsmovements", LinkTableSQLName = "productsserialsmovements", ColumnReference1Name = "idproductserial", ColumnReference2Name = "idproductmovement")]
        [RenderInfo(DependencyFilterPropertyName = "Product", ParentFilterPropertyName = "IdProduct", PageGroup = "Serials")]
        public List<ProductSerial> InboundProductSerials { get; set; }


         
        /// NO
        /// se serve usa i get
        //public override void Loaded(BaseModel parentItem = null)
        //{
        //    UnitVat = UnitNetPrice * (VatPercentage / 100);
        //    UnitGrossPrice = UnitNetPrice + UnitVat;
        //    GrossPrice = UnitGrossPrice * Quantity;

        //    ProductSerialsString = string.Empty;
        //    foreach (var serial in InboundProductSerials ?? new List<ProductSerial>())
        //    {
        //        ProductSerialsString += serial?.SerialCode + ",";
        //    }

        //    MissingSerialsQuantity = Quantity - InboundProductSerials?.Count ?? 0;

        //    base.Loaded(parentItem);
        //}

        //public override void Updated(BaseModel parentItem = null)
        //{
        //    if (Product?.ID != IdProduct)
        //    {
        //        Description = Product?.Description;
        //        Code = Product?.MainCode;
        //        VatPercentage = Product?.DefaultVat ?? 0;
        //        UnitNetPrice = Product?.DefaultSellPrice ?? 0;
        //        UnitPurchaseNetPrice = Product?.DefaultPurchasePrice ?? 0;
        //    }

        //    MissingSerialsQuantity = Quantity - InboundProductSerials?.Count ?? 0;

        //    base.Updated(parentItem);
        //}
    }
}