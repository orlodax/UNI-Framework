using System;
using System.Collections.Generic;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.Library.Mapping;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "ddts")]
    public class DDT : BaseModel
    {

        [ValueInfo(SQLName = "protocol", IsDisplayProperty = true)]
        public string Protocol { get; set; }

        //[ValueInfo(SQLName = "series")]  public string Series { get; set; }

        [ValueInfo(SQLName = "description")]
        public string Description { get; set; }


        [ValueInfo(SQLName = "state")]
        [RenderInfo(IsFixedValue = true, NewLine = true)]
        public string State { get; set; } = "Da registrare";


        [ValueInfo(SQLName = "causal")]
        public string Causal { get; set; }

        [ValueInfo(SQLName = "sender")]
        [RenderInfo(Group = "Info")]
        public string Sender { get; set; }


        [ValueInfo(SQLName = "titleholder")]
        [RenderInfo(Group = "Info")]
        public string TitleHolder { get; set; }


        [ValueInfo(SQLName = "purchaseorderref")]
        [RenderInfo(Group = "Info", NewLine = true)]
        public string PurchaseOrderRef { get; set; }


        [ValueInfo(SQLName = "salesorderref")]
        [RenderInfo(Group = "Info")]
        public string SalesOrderRef { get; set; }


        [ValueInfo(SQLName = "carrier")]
        [RenderInfo(Group = "Shipping")]
        public string Carrier { get; set; }


        [ValueInfo(SQLName = "warehouse")]
        [RenderInfo(Group = "Shipping")]
        public string Warehouse { get; set; }


        [ValueInfo(SQLName = "departure")]
        [RenderInfo(Group = "Shipping", NewLine = true)]
        public string Departure { get; set; }


        [ValueInfo(SQLName = "destination")]
        [RenderInfo(Group = "Shipping")]
        public string Destination { get; set; }


        [ValueInfo(SQLName = "docdate")]
        [RenderInfo(Group = "Shipping", NewLine = true)]
        public DateTime DocDate { get; set; }


        [ValueInfo(SQLName = "docnumber")]
        [RenderInfo(Group = "Shipping")]
        public string DocNumber { get; set; }


        [ValueInfo(SQLName = "note")]
        [RenderInfo(Group = "Shipping", NewLine = true)]
        public string Note { get; set; }

        //[ValueInfo(SQLName = "starttime")] [RenderInfo(Group = "Document", NewLine = true)] public DateTime StartTime { get; set; }
        //[ValueInfo(SQLName = "endtime")] [RenderInfo(Group = "Document")]  public DateTime EndTime { get; set; }
        //[ValueInfo(SQLName = "packagesnumber")] [RenderInfo(Group = "Price")] public string PackagesNumber { get; set; }
        //[ValueInfo(SQLName = "paymentmethod")] [RenderInfo(Group = "Price")]  public string PaymentMethod { get; set; }
        //[ValueInfo(SQLName = "netprice")] [RenderInfo(Group = "Price", NewLine = true)] public string NetPrice { get; set; }
        //[ValueInfo(SQLName = "grossprice")] [RenderInfo(Group = "Price")] public string GrossPrice { get; set; }
        //[ValueInfo(SQLName = "idappuser")] [RenderInfo(Group = "Price")]  public int IdAppUser { get; set; }

        [ValueInfo()]
        public AppUser AppUser { get; set; }


        [ValueInfo()]
        [RenderInfo(PageGroup = "Movimenti magazzino")]
        public List<ProductMovement> ProductMovements { get; set; }

    }
}
