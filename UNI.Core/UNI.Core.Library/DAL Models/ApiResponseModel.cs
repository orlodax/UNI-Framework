using System.Collections;
using System.Collections.Generic;

namespace UNI.Core.Library.GenericModels
{
    public class ApiResponseModel<T>
    {
        public int Count { get; set; }
        public int DataBlocks { get; set; }
        public List<T> ResponseBaseModels { get; set; } = new List<T>();
        public Dictionary<string, IList> BaseModelDependencies { get; set; } = new Dictionary<string, IList>();
    }
}
