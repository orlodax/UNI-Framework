using System.Collections.Generic;
using UNI.Core.Library;
using UNI.Core.UI.Tabs.DetailItem;

namespace UNI.Core.Explorer.ViewModels.DetailItem
{
    internal class DetailItemInboundDDTVB<T> : DetailItemVB<T> where T : BaseModel
    {
        public DetailItemInboundDDTVB()
        {
            VisibleProperties = new List<string>()
            {
                "Number",
                "Date",
                "TransportDate",
                "Employee",
                "State",
                "Sender",
                "Departure",
                "Destination",
                "Appereance",
                "Causal",
                "ExpeditionType",
                "PackagesNumber",
                "PaymentMethod",
                "Port",
                "Carrier",
                "Notes",
                "InboundDDTRows",
                "NetPrice",
                "VatPrice",
                "GrossPrice"
            };
        }
    }
}
