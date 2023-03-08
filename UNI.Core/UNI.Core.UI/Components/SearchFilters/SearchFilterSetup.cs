using System;
using System.Collections.Generic;

namespace UNI.Core.UI.Components.SearchFilters
{
    public class SearchFilterSetup
    {
        /// <summary>
        /// Refers to EnTimeRange to select day, month, year...
        /// </summary>
        public int? TimeRange { get; set; } = null;

        /// <summary>
        /// Which date should appear on loading in the search filters control
        /// </summary>
        public DateTimeOffset? InitialDate { get; set; } = null;

        /// <summary>
        /// Which field should be used on loading in the search filters control
        /// </summary>
        public string InitialProperty { get; set; } = null;

        /// <summary>
        /// Specify manually which properties to show in the SearchFilters combobox
        /// </summary>
        public List<string> PropertiesToShow { get; set; }
    }
}
