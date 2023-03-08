using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UNI.Core.Library;
using UNI.Core.UI.Misc.SearchFilter;
using UNI.Core.UI.Tabs;
using Windows.ApplicationModel.Resources;

namespace UNI.Core.UI.CustomControls.DateFilter
{

    // TODO perché non è usato?

    public class DateFilterVM<T> : BaseTabVMTypeAgnostic
    {
        private List<string> timeRanges = new List<string>();
        private string selectedRange;
        private PropertyInfo[] dateProperties;
        private PropertyInfo selectedDateProperty;
        private DateTimeOffset fromDate;
        private DateTimeOffset toDate;
        private bool isDatePickerEnabled;

        public List<string> TimeRanges { get => timeRanges; set => SetValue(ref timeRanges, value); }
        public string SelectedRange { get => selectedRange; set { SetValue(ref selectedRange, value); TimeRangeChanged(); } }
        public PropertyInfo[] DateProperties { get => dateProperties; set => SetValue(ref dateProperties, value); }
        public PropertyInfo SelectedDateProperty { get => selectedDateProperty; set => SetValue(ref selectedDateProperty, value); }
        public DateTimeOffset FromDate { get => fromDate; set => SetValue(ref fromDate, value); }
        public DateTimeOffset ToDate { get => toDate; set => SetValue(ref toDate, value); }
        public event EventHandler TimeRangeChangedEvent;

        //this controls the visibility of the two datepickers. It is enabled if the user selects custom date search (dateFilter_Ranges_5)
        public bool IsDatePickerEnabled { get => isDatePickerEnabled; set => SetValue(ref isDatePickerEnabled, value); }


        /// <summary>
        /// Since we use /Strings, we need to map the dateFilter_Ranges_{index} keys to desired logic
        /// </summary>
        readonly Dictionary<string, DateRange> RangesDescription = new Dictionary<string, DateRange>();
        readonly ResourceLoader resourceLoader = new ResourceLoader();
        readonly SearchFilter<T> searchFilter = new SearchFilter<T>();

        public DateFilterVM()
        {
            // initiate the controls on today
            FromDate = ToDate = DateTimeOffset.Now;

            // feed the (datetime)property selector combobox 
            DateProperties = typeof(T).GetProperties().Where(p => p.PropertyType == typeof(DateTime)).ToArray();

            //select as default a property if specified in valueinfo
            foreach (var property in dateProperties)
            {
                if (property.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                {
                    if (valueInfo.IsDefaultDataFilter)
                        selectedDateProperty = property;
                }
            }



            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;
            int day = DateTime.Today.Day;
            DateTime todayStart = new DateTime(year, month, day);
            DateTime todayEnd = new DateTime(year, month, day).AddHours(24);

            RangesDescription.Add("dateFilter_Ranges_0", new DateRange(todayStart, todayEnd));
            RangesDescription.Add("dateFilter_Ranges_1", new DateRange(todayStart.AddHours(-24), todayStart));
            //RangesDescription.Add("dateFilter_Ranges_2", new DateRange(todayStart.AddDays(-7), todayEnd));
            RangesDescription.Add("dateFilter_Ranges_2", new DateRange(new DateTime(year, month, 1).AddMonths(-1), new DateTime(year, month, 1).AddHours(-1)));
            RangesDescription.Add("dateFilter_Ranges_3", new DateRange(new DateTime(year - 1, 1, 1), new DateTime(year - 1, 12, 31).AddHours(24)));
            //RangesDescription.Add("dateFilter_Ranges_5", new DateRange(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday), todayEnd)); //this week
            RangesDescription.Add("dateFilter_Ranges_4", new DateRange(new DateTime(year, month, 1), new DateTime(year, month, 1).AddDays(DateTime.DaysInMonth(year, month)))); //this month
            RangesDescription.Add("dateFilter_Ranges_5", new DateRange(new DateTime(year, 1, 1), new DateTime(year, 12, 31).AddHours(24))); //this year

            // read time ranges from /Strings
            bool notBreak = true;
            int i = 0;
            do
            {
                string entry = resourceLoader.GetString($"dateFilter_Ranges_{i}");
                i++;
                if (!string.IsNullOrWhiteSpace(entry))
                    TimeRanges.Add(entry);
                else
                    notBreak = false;
            }
            while (notBreak);

            //select this Year as default
            SelectedRange = resourceLoader.GetString($"dateFilter_Ranges_5");
        }

        // on combobox rangeSelector selection changed
        void TimeRangeChanged()
        {

            int rangeIndex = TimeRanges.IndexOf(SelectedRange);
            // 5 is in between custom dates
            //if (rangeIndex == 5)
            //{
            //    IsDatePickerEnabled = true;
            //}
            //else
            //{
            IsDatePickerEnabled = false;
            RangesDescription.TryGetValue($"dateFilter_Ranges_{rangeIndex}", out DateRange dateRange);
            FromDate = dateRange.StartDate;
            ToDate = dateRange.EndDate;
            //}
            TimeRangeChangedEvent?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Main method called by cointaining view's VM 
        /// </summary>
        public List<T> PerformSearch(List<T> inputList, string searchText)
        {
            PropertyInfo[] datePropertiesToSearchInto;

            if (SelectedDateProperty == null)
                datePropertiesToSearchInto = DateProperties;
            else
                datePropertiesToSearchInto = new PropertyInfo[] { SelectedDateProperty };

            var preFilteredList = searchFilter.PerformSearch(inputList, searchText, typeof(T).GetProperties());
            var filteredList = new List<T>();

            foreach (var item in preFilteredList)
            {
                foreach (var dateProperty in datePropertiesToSearchInto)
                {
                    var val = (DateTime)dateProperty.GetValue(item);
                    if (val >= FromDate && val <= ToDate)
                        if (!filteredList.Contains(item))
                            filteredList.Add(item);
                }
            }

            return filteredList;
        }
    }
}
