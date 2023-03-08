using UNI.Core.Library;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "sectors")]
    public class Sector : BaseModel
    {
        [ValueInfo(SQLName = "name")]
        public string Name { get; set; }


        [ValueInfo(SQLName = "code")]
        public string Code { get; set; }
    }
}
