using System;
using System.Collections.Generic;
using System.Reflection;
using UNI.Core.Library;

namespace UNI.Core.UI.Misc.SearchFilter
{
    public class SearchFilter<T>
    {
        readonly List<T> backupList = new List<T>();

        public List<T> PerformSearch(List<T> inputList, string searchText, PropertyInfo[] properties = null)
        {
            // if no properties are specified, look in all
            if (properties == null)
                properties = typeof(T).GetProperties();

            this.backupList.Clear();
            // runs first time, stores a copy not more needed cause of notfiltered insied basecrudvm
            //if (!backupList.Any())
            foreach (var item in inputList)
                this.backupList.Add(item);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var filteredList = new List<T>();

                // iterate through items of inputlist to be filtered
                foreach (var baseModel in inputList)
                {
                    // iterate through properties of that basemodel complete type
                    foreach (var property in properties ?? new PropertyInfo[] { })
                    {
                        // exclude lists
                        if (!property.PropertyType.IsGenericType)
                        {
                            // is it another basemodel or a value property?
                            if (property.PropertyType.IsSubclassOf(typeof(BaseModel)))
                            {
                                if (SearchValueInObject(searchText, property.GetValue(baseModel)))
                                {
                                    filteredList.Add(baseModel);
                                    break;
                                }
                            }
                            else //value property
                            {
                                var val = property.GetValue(baseModel);
                                if (val != null)
                                {
                                    string value = val.ToString() ?? string.Empty;
                                    if (value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                    {
                                        filteredList.Add(baseModel);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                return filteredList;
            }
            return backupList;
        }
        bool SearchValueInObject(string searchText, object item)
        {
            if (item != null)
            {
                Type t = item.GetType();
                foreach (var property in t.GetProperties() ?? new PropertyInfo[] { })
                {
                    var val = property.GetValue(item);
                    if (val != null)
                    {
                        string value = val.ToString() ?? string.Empty;
                        if (value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
