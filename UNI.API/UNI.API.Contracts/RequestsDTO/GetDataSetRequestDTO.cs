using System.Collections.Generic;
using UNI.Core.Library;

namespace UNI.API.Contracts.RequestsDTO
{
    public class GetDataSetRequestDTO
    {
        public int? Id { get; set; }
        public string IdName { get; set; } 
        public int? RequestedEntriesNumber { get; set; }
        public int? BlockToReturn { get; set; } 
        public string FilterText { get; set; } 
        public string TableAttribute { get; set; }
        public bool SkipInit { get; set; } 
        public bool LargeTablesLogic { get; set; }
        public bool BackendDependencyResolve { get; set; }


        public List<FilterExpression> FilterExpressions { get; set; } = new List<FilterExpression>();

        public string FilterDateFormat { get; set; } = "%Y-%m-%d";
    }
}
