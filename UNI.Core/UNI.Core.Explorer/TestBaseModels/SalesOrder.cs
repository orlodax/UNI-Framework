using System;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.Library.Mapping;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "salesorders")]
    public class SalesOrder : BaseModel
    {
        [ValueInfo(SQLName = "description")]
        public string Description { get; set; }


        [ValueInfo(SQLName = "sender", IsDisplayProperty = true)]
        public string Sender { get; set; }


        [ValueInfo(SQLName = "date")]
        [RenderInfo(NewLine = true)]
        public DateTime Date { get; set; }


        [ValueInfo(SQLName = "state")]
        [RenderInfo(IsFixedValue = true)]
        public string State { get; set; } = "Da confermare";


        [ValueInfo(SQLName = "protocol")]
        [RenderInfo(Group = "Info")]
        public string Protocol { get; set; }


        [ValueInfo(SQLName = "registrationnumber")]
        [RenderInfo(Group = "Info")]
        public string RegistrationNumber { get; set; }


        [ValueInfo(SQLName = "idddt")]
        [RenderInfo(Group = "Info", NewLine = true)]
        public int IdDDT { get; set; }


        [ValueInfo(SQLName = "series")]
        [RenderInfo(Group = "Info")]
        public string Series { get; set; }


        [ValueInfo(SQLName = "causal")]
        [RenderInfo(Group = "Info", NewLine = true)]
        public string Causal { get; set; }


        [ValueInfo(SQLName = "quantity")]
        [RenderInfo(Group = "Info")]
        public string Quantity { get; set; }


        [ValueInfo(SQLName = "packagesnumber")]
        [RenderInfo(Group = "Info", NewLine = true)]
        public string PackagesNumber { get; set; }


        [ValueInfo(SQLName = "totdoc")]
        [RenderInfo(Group = "Info")]
        public string TotDoc { get; set; }


        [ValueInfo(SQLName = "destination")]
        [RenderInfo(Group = "Shipping")]
        public string Destination { get; set; }


        [ValueInfo(SQLName = "warehouse")]
        [RenderInfo(Group = "Shipping")]
        public string Warehouse { get; set; }


        [ValueInfo(SQLName = "departure")]
        [RenderInfo(Group = "Shipping", NewLine = true)]
        public string Departure { get; set; }


        [ValueInfo(SQLName = "deliverydate")]
        [RenderInfo(Group = "Shipping")]
        public DateTime DeliveryDate { get; set; }


        [ValueInfo(SQLName = "idbordero")]
        [RenderInfo(Group = "Shipping", NewLine = true)]
        public int IdBordero { get; set; }


        [ValueInfo(SQLName = "note")]
        [RenderInfo(Group = "Shipping")]
        public string Note { get; set; }


        [ValueInfo()]
        public DDT DDT { get; set; }


        [ValueInfo()]
        [RenderInfo(Group = "Shipping")]
        public UniDataSet<SalesOrderRow> SalesOrderRows { get; set; }
    }

}
