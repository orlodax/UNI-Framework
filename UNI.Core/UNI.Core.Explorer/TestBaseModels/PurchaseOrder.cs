using System;
using System.Collections.Generic;
using UNI.Core.Library;
using UNI.Core.Library.Mapping;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "purchaseorders")]
    public class PurchaseOrder : BaseModel
    {
        /// <summary>
        /// Incoming deliveries log
        /// Information used for writing the ddt
        /// </summary>
        /// 
        [ValueInfo(SQLName = "protocol", IsDisplayProperty = true)]
        public string Protocol { get; set; }


        [ValueInfo(SQLName = "sender")]
        public string Sender { get; set; }


        [ValueInfo(SQLName = "state")]
        [RenderInfo(IsFixedValue = true, NewLine = true)]
        public string State { get; set; } = "Da registrare";


        [ValueInfo(SQLName = "description")]
        public string Description { get; set; }


        [ValueInfo(SQLName = "registrationnumber")]
        [RenderInfo(Group = "Info")]
        public string RegistrationNumber { get; set; }


        [ValueInfo(SQLName = "idddt")]
        [RenderInfo(Group = "Info")]
        public int IdDDT { get; set; }


        [ValueInfo(SQLName = "series")]
        [RenderInfo(Group = "Info", NewLine = true)]
        public string Series { get; set; }


        [ValueInfo(SQLName = "causal")]
        [RenderInfo(Group = "Info")]
        public string Causal { get; set; }


        [ValueInfo(SQLName = "packagesnumber")]
        [RenderInfo(Group = "Info", NewLine = true)]
        public string PackagesNumber { get; set; }


        [ValueInfo(SQLName = "totdoc")]
        [RenderInfo(Group = "Info")]
        public string TotDoc { get; set; }


        [ValueInfo(SQLName = "destination")]
        [RenderInfo(Group = "Shipping")]
        public string Destination { get; set; }


        [ValueInfo(SQLName = "departure")]
        [RenderInfo(Group = "Shipping")]
        public string Departure { get; set; }


        [ValueInfo(SQLName = "date")]
        [RenderInfo(Group = "Shipping", NewLine = true)]
        public DateTime Date { get; set; }


        [ValueInfo(SQLName = "deliverydate")]
        [RenderInfo(Group = "Shipping")]
        public DateTime DeliveryDate { get; set; }


        [ValueInfo(SQLName = "warehouse")]
        [RenderInfo(Group = "Shipping", NewLine = true)]
        public string Warehouse { get; set; }


        [ValueInfo(SQLName = "note")]
        [RenderInfo(Group = "Shipping")]
        public string Note { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public string DeliveryDateText { get { return DeliveryDate.ToShortDateString(); } }


        [ValueInfo()]
        public DDT DDT { get; set; }


        [ValueInfo()]
        [RenderInfo(Group = "Shipping")]
        public List<PurchaseOrderRow> PurchaseOrderRows { get; set; }


    }
}
