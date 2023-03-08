using System.Collections.Generic;
using UNI.Core.Library.Converters;
using UNI.Core.Library.Mapping;

namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "productsmovements")]
    public class OutboundDDTRow : BaseProductMovement
    {
        [ValueInfo(SQLName = "unitnetprice")][RenderInfo(Converter = typeof(CurrencyConverter))] public double UnitNetPrice { get; set; }
        [ValueInfo(SQLName = "unitpurchasenetprice")][RenderInfo(Converter = typeof(CurrencyConverter))] public double UnitPurchaseNetPrice { get; set; }
        [ValueInfo(SQLName = "vatpercentage")][RenderInfo(Converter = typeof(PercentageConverter))] public double VatPercentage { get; set; }
        [ValueInfo(IsReadOnly = true)][RenderInfo(Converter = typeof(CurrencyConverter))] public double UnitVat { get; set; }
        [ValueInfo(IsReadOnly = true)][RenderInfo(Converter = typeof(CurrencyConverter))] public double UnitGrossPrice { get; set; }
        [ValueInfo(IsReadOnly = true)][RenderInfo(Converter = typeof(CurrencyConverter))] public double GrossPrice { get; set; }
        [ValueInfo(SQLName = "nominalquantity")] public double NominalQuantity { get; set; }

        [ValueInfo(ManyToManySQLName = "viewproductsserialsmovements", LinkTableSQLName = "productsserialsmovements", ColumnReference1Name = "idproductserial", ColumnReference2Name = "idproductmovement")]
        [RenderInfo(DependencyFilterPropertyName = "Product", ParentFilterPropertyName = "IdProduct", PageGroup = "Serials")]
        public List<ProductSerial> OutboundProductSerials { get; set; }


        /// NO
        /// se serve usa i get
        //public override void Loaded(BaseModel parentItem = null)
        //{
        //    Quantity = NominalQuantity * -1;
        //    UnitVat = UnitNetPrice * (VatPercentage / 100);
        //    UnitGrossPrice = UnitNetPrice + UnitVat;
        //    GrossPrice = UnitGrossPrice * NominalQuantity;

        //    ProductSerialsString = string.Empty;
        //    foreach (var serial in OutboundProductSerials ?? new List<ProductSerial>())
        //    {
        //        ProductSerialsString += serial.SerialCode + ",";
        //    }

        //    MissingSerialsQuantity = Quantity + OutboundProductSerials?.Count ?? 0;


        //    base.Loaded(parentItem);
        //}

        //public override void Updated(BaseModel parentItem = null)
        //{
        //    if (Product != null)
        //    {
        //        if (Product.ID != IdProduct)
        //        {
        //            Description = Product.Description;
        //            Code = Product.MainCode;
        //            VatPercentage = Product.DefaultVat;
        //            UnitNetPrice = Product.DefaultSellPrice;
        //            UnitPurchaseNetPrice = Product.DefaultPurchasePrice;
        //        }
        //    }

        //    MissingSerialsQuantity = Quantity + OutboundProductSerials?.Count ?? 0;

        //    base.Updated(parentItem);
        //}
    }
}