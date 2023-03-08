using UNI.Core.Library;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "slotstatus")]
    public class SlotStatus : Product
    {
        [ValueInfo(SQLName = "quantity")]
        public double QuantitySlot { get; set; }


        [ValueInfo(SQLName = "idslot")]
        public int IdSlot { get; set; }

        public string CoordinataEstesa { get; set; }

        //[ValueInfo()] public Product Product { get; set; }

        [ValueInfo(IsReadOnly = true)]
        public double PlatformQuantitySlot { get; set; }

        public SlotStatus()
        {
            if (PlatformQuantity != 0)
                PlatformQuantitySlot = QuantitySlot / PlatformQuantity;
        }
    }
}
