using System.Collections.Generic;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.Library.Mapping;

namespace Gioiaspa.Warehouse.Library
{
    [ClassInfo(SQLName = "users", ClassType = "AppUserWms")]
    public class AppUserWms : AppUser
    {
        //[ValueInfo(SQLName = "code")] public string WarehouseId { get; set; }

        [ValueInfo(ManyToManySQLName = "viewwarehousesusers", LinkTableSQLName = "userswarehouses")]
        [RenderInfo(Group = "")]
        public List<Warehouse> Warehouses { get; set; }


        [ValueInfo(SQLName = "idsector", IsDisplayProperty = true)]
        public int IdSector { get; set; }


        [ValueInfo()]
        public Sector Sector { get; set; }
    }
}
