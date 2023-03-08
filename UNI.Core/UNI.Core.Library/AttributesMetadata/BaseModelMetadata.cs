using System.Collections.Generic;

namespace UNI.Core.Library.AttributesMetadata
{
    /// <summary>
    /// This class will hold all configurations that were done previously by multiple attributes
    /// </summary>
    public class BaseModelMetadata
    {
        public ClassAttributes ClassAttributes { get; set; } = new ClassAttributes();
        public Dictionary<string, GraphicAttributes> GraphicAttributes { get; } = new Dictionary<string, GraphicAttributes>();
        public Dictionary<string, DataAttributes> DataAttributes { get; } = new Dictionary<string, DataAttributes>();
    }
}
