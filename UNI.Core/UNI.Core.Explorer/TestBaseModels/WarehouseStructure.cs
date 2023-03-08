using System.Collections.Generic;
using UNI.Core.Library;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "warehousestructure")]
    public class WarehouseStructure : BaseModel
    {
        [ValueInfo(SQLName = "name")]
        public string Code { get; set; }


        [ValueInfo(SQLName = "idwarehousestructure")]
        public int IdWarehouseStructure { get; set; }


        [ValueInfo(SQLName = "idwarehouse")]
        public int IdWarehouse { get; set; }


        [ValueInfo(SQLName = "x")]
        public string X { get; set; }


        [ValueInfo(SQLName = "y")]
        public string Y { get; set; }


        [ValueInfo(SQLName = "z")]
        public string Z { get; set; }

        [ValueInfo()]
        public List<WarehouseStructure> WarehouseStructures { get; set; }

    }
}
