using UNI.Core.Library;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "warehouses")]
    public class Warehouse : BaseModel
    {
        [ValueInfo(SQLName = "code")]
        public string Code { get; set; }
    }
}
