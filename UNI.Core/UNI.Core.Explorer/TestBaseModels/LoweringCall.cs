using System;
using UNI.Core.Library;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "loweringcalls")]
    public class LoweringCall : BaseModel
    {
        [ValueInfo(SQLName = "state")]
        public string State { get; set; } = "Da abbassare";


        [ValueInfo(SQLName = "idslot_from")]
        public int IdProductSlotStart { get; set; }


        [ValueInfo(SQLName = "idslot_to")]
        public int IdProductSlotEnd { get; set; }


        [ValueInfo(SQLName = "date")]
        public DateTime Date { get; set; }


        [ValueInfo(SQLName = "idproduct", IsVisible = false)]
        public int IdProduct { get; set; }

        public string CoordinataEstesa { get; set; }

        [ValueInfo()]
        public Product Product { get; set; }
        // id slot to, id slot from, product, state, date, id, 
    }
}
