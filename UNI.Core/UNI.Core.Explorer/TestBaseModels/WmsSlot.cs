using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.Library.Mapping;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "wmsslots")]
    public class WmsSlot : BaseModel
    {
        [ValueInfo(SQLName = "classtype", IsDisplayProperty = true)]
        public string ClassType { get; set; }


        [ValueInfo(SQLName = "maxquantity")]
        public int MaxQuantity { get; set; }


        [ValueInfo(SQLName = "idwarehouse")]
        [RenderInfo(NewLine = true)]
        public int IdWarehouse { get; set; }


        [ValueInfo(SQLName = "idum")]
        public int Idum { get; set; }


        [ValueInfo(SQLName = "coord1")]
        [RenderInfo(NewLine = true)]
        public string Coord1 { get; set; }


        [ValueInfo(SQLName = "coord2")]
        public string Coord2 { get; set; }


        [ValueInfo(SQLName = "coord3")]
        [RenderInfo(NewLine = true)]
        public string Coord3 { get; set; }


        [ValueInfo(SQLName = "coord4")]
        public string Coord4 { get; set; }


        [ValueInfo(SQLName = "ispicking")]
        [RenderInfo(NewLine = true)]
        public bool IsPicking { get; set; }


        [ValueInfo()]
        public UniDataSet<SlotStatus> SlotStatus { get; set; }
    }
}
