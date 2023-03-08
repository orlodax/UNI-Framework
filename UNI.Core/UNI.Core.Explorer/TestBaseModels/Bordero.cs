using System;
using System.Collections.Generic;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.Library.Mapping;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "borderos")]
    public class Bordero : BaseModel
    {
        /// <summary>
        /// Nome autista
        /// </summary>
        [ValueInfo(SQLName = "name")]
        public string Name { get; set; }


        /// <summary>
        /// Vettore
        /// </summary>
        [ValueInfo(SQLName = "carrier", IsDisplayProperty = true)]
        public string Carrier { get; set; }


        /// <summary>
        /// Area di competenza
        /// </summary>
        [ValueInfo(SQLName = "area")]
        [RenderInfo(NewLine = true)]
        public string Area { get; set; }


        /// <summary>
        /// Stato
        /// </summary>
        [ValueInfo(SQLName = "state")]
        [RenderInfo(IsFixedValue = true)]
        public string State { get; set; } = "Da controllare";


        [ValueInfo(SQLName = "date")]
        [RenderInfo(NewLine = true)]
        public DateTime Date { get; set; }


        /// <summary>
        /// Data di consegna
        /// </summary>
        [ValueInfo(SQLName = "deliverydate")]
        public DateTime DeliveryDate { get; set; }


        /// <summary>
        /// Magazzino
        /// </summary>
        [ValueInfo(SQLName = "warehouse")]
        [RenderInfo(NewLine = true)]
        public string Warehouse { get; set; }


        [ValueInfo(SQLName = "note")]
        public string Notes { get; set; }



        [ValueInfo(ManyToManySQLName = "viewsectorsborderos", LinkTableSQLName = "borderosectors")]
        [RenderInfo(Group = "")]
        public List<Sector> Sectors { get; set; }


        [ValueInfo(IsReadOnly = true)]
        public string DateText { get { return Date.ToShortDateString(); } }


        [ValueInfo()]
        [RenderInfo(Group = "Order")]
        public UniDataSet<SalesOrder> SalesOrder { get; set; }

    }
}
