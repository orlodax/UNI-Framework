using System;

namespace UNI.Core.Library.AttributesMetadata
{
    /// <summary>
    /// Holds all "attributes" consumed by the UWP frontend (previously contained in RenderInfo)
    /// </summary>
    public class GraphicAttributes
    {
        /// <summary>
        /// False = Must not be rendered in views
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// TODO: che è?
        /// </summary>
        public bool IsFixedValue { get; set; }

        /// <summary>
        /// TODO che differenza c'è con PageGroup? Name of the group of properties, used for rendering
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// TODO che differenza c'è con Group? Name of the group of properties, used for rendering
        /// </summary>
        public string PageGroup { get; set; }

        /// <summary>
        /// Which converter to utilize for the property. 
        /// </summary>
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

        /// <summary>
        /// Set if in a datagrid generate an editable combobox
        /// </summary>
        public bool IsDataGridEditable { get; set; }

        /// <summary>
        /// TODO: naming violation. "StartOnNewLine". Set if the control must start on a new line in view
        /// </summary>
        public bool NewLine { get; set; }

        /// <summary>
        /// Marks the property as the one to be displayed in ShowBoxes, ComoboBoxes, etc
        /// </summary>
        public bool IsDisplayProperty { get; set; }
    }
}
