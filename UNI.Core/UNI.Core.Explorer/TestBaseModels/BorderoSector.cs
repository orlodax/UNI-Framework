using UNI.Core.Library;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "borderosectors")]
    public class BorderoSector : BaseModel
    {
        [ValueInfo(SQLName = "idbordero")]
        public int IdBordero { get; set; }


        [ValueInfo(SQLName = "idsector")]
        public int IdSector { get; set; }

    }
}
