namespace UNI.Core.UI.CustomControls.ShowBox
{
    public class ShowBoxFilters
    {
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
    }
}
