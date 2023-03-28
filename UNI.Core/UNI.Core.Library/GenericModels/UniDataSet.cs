using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UNI.Core.Library.GenericModels
{
    public class UniDataSet<T>
    {
        public int Count { get; set; }
        public int DataBlocks { get; set; }

        public virtual Task<List<T>> Get(int? id = null, string idName = null, int? requestedEntriesNumber = 50, int blockToReturn = 1, string filterText = null, bool skipInit = false, List<FilterExpression> filterExpressions = null)
        {
            //filterExpressions = filterExpressions ?? new List<FilterExpression>();
            throw new NotImplementedException();
        }
        public virtual object Query(string query)
        {
            throw new NotImplementedException();
        }
    }
}