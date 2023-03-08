using System;
using System.Collections.Generic;
using UNI.Core.Library.Mapping;

namespace UNI.Core.Library.GenericModels
{
    [ClassInfo(SQLName = "inboundddts")]
    public class InboundDDT : BaseModel
    {
        [ValueInfo(SQLName = "number", IsDisplayProperty = true)] public string Number { get; set; }
        [ValueInfo(SQLName = "date")] public DateTime Date { get; set; } = DateTime.Today;
        [ValueInfo(SQLName = "transportdate")] public DateTime TransportDate { get; set; } = DateTime.Today;
        [ValueInfo(SQLName = "state")][RenderInfo(IsFixedValue = true)] public string State { get; set; }
        [ValueInfo(SQLName = "sender")] public string Sender { get; set; }
        [ValueInfo(SQLName = "departure")][RenderInfo(NewLine = true)] public string Departure { get; set; }
        [ValueInfo(SQLName = "destination")] public string Destination { get; set; }
        [ValueInfo(SQLName = "appereance")] public string Appereance { get; set; }
        [ValueInfo(SQLName = "causal")] public string Causal { get; set; }
        [ValueInfo(SQLName = "expeditiontype")] public string ExpeditionType { get; set; }
        [ValueInfo(SQLName = "packagesnumber")][RenderInfo(NewLine = true)] public string PackagesNumber { get; set; }
        [ValueInfo(SQLName = "paymentmethod")] public string PaymentMethod { get; set; }
        [ValueInfo(SQLName = "port")] public string Port { get; set; }
        [ValueInfo(SQLName = "carrier")] public string Carrier { get; set; }
        [ValueInfo(SQLName = "notes")] public string Notes { get; set; }
        [ValueInfo(SQLName = "idemployee")] public int IdEmployee { get; set; }
        [ValueInfo(IsReadOnly = true)] public double NetPrice { get; set; }
        [ValueInfo(IsReadOnly = true)] public double VatPrice { get; set; }
        [ValueInfo(IsReadOnly = true)] public double GrossPrice { get; set; }
        [ValueInfo()] public Employee Employee { get; set; }
        [ValueInfo()] public List<InboundDDTRow> InboundDDTRows { get; set; }

        public InboundDDT()
        {
            NetPrice = 0;
            VatPrice = 0;
            GrossPrice = 0;
            foreach (var row in InboundDDTRows ?? new List<InboundDDTRow>())
            {
                //row.Loaded();
                NetPrice += row.UnitNetPrice * row.Quantity;
                VatPrice += row.UnitVat * row.Quantity;
                GrossPrice += row.GrossPrice;
            }

        }

    }
}