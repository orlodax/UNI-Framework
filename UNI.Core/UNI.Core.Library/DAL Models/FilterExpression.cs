using System.Collections.Generic;

namespace UNI.Core.Library
{
    public class FilterExpression
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public string OperatorSign { get; set; } = "="; 
        public string ComparisonType { get; set; } = "AND";
        public List<FilterExpression> FilterExpressions { get; set; } = new List<FilterExpression>();
    }
}