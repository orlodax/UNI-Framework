using System;

namespace UNI.Core.Library.Mapping
{
    public class RenderInfo : Attribute
    {
        public bool IsFixedValue { get; set; } = false;
        public string Group { get; set; } = string.Empty;                           // Name of the group of properties, used for rendering
        public string PageGroup { get; set; } = string.Empty;                       // Name of the group of properties, used for rendering
        public Type Converter { get; set; }

        /// <summary>
        /// The object property based on showbox filter the possible data. The property must be present in both Dependency and base object 
        /// </summary>
        public string DependencyFilterPropertyName { get; set; }

        /// <summary>
        /// The parent property based on showbox filter the possible data. The property must be present in both Dependency and base object
        /// </summary>
        public string ParentFilterPropertyName { get; set; }

        /// <summary>
        /// The value based on showbox filter the possible data. 
        /// </summary>
        public string DependencyFilterPropertyValue { get; set; }

        public bool IsDataGridEditable { get; set; } = false;                       // Set if in a datagrid generate an editable combobox
        public bool NewLine { get; set; } = false;                                  // Set if the control must start on a new line in view
    }
}