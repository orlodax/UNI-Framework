using System;
using System.Collections.Generic;
using UNI.Core.Library.Mapping;

namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "productsmovements")]
    public class BaseProductMovement : BaseModel
    {
        [ValueInfo()][RenderInfo(DependencyFilterPropertyName = "IsService", DependencyFilterPropertyValue = "0")] public BaseProduct Product { get; set; }
        [ValueInfo(SQLName = "idproduct")] public int IdProduct { get; set; }
        [ValueInfo(SQLName = "idinboundddt")] public int IdInboundDDT { get; set; }
        [ValueInfo(SQLName = "idoutboundddt")] public int IdOutboundDDT { get; set; }

        [ValueInfo(SQLName = "code")] public string Code { get; set; }
        [ValueInfo(SQLName = "description")] public string Description { get; set; }
        [ValueInfo(SQLName = "quantity")] public double Quantity { get; set; }


        [ValueInfo(ManyToManySQLName = "viewproductsserialsmovements", LinkTableSQLName = "productsserialsmovements")]
        [RenderInfo(DependencyFilterPropertyName = "Product", ParentFilterPropertyName = "IdProduct")]
        public List<ProductSerial> ProductSerials { get; set; }
        [ValueInfo(IsReadOnly = true)] public string ProductSerialsString { get; set; }

        [ValueInfo(SQLName = "date", ParentPropertyDependendency = "TransportDate")] public DateTime Date { get; set; }
        [ValueInfo(IsReadOnly = true)] public InboundDDT InboundDDT { get; set; }
        [ValueInfo(IsReadOnly = true)] public OutboundDDT OutboundDDT { get; set; }
        [ValueInfo(IsReadOnly = true)][RenderInfo(PageGroup = "Serials")] public double MissingSerialsQuantity { get; set; }
        [ValueInfo(IsReadOnly = true)] public string DocumentReference { get; set; }

        public BaseProductMovement()
        {
            ProductSerialsString = string.Empty;
            foreach (var serial in ProductSerials ?? new List<ProductSerial>())
            {
                ProductSerialsString += serial.SerialCode + ",";
            }

            if (InboundDDT != null)
                DocumentReference = $"{InboundDDT.Number}";
            else
            if (OutboundDDT != null)
                DocumentReference = $"{OutboundDDT.Number}";
        }

    }
}