using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Collections.Generic;
using System.Reflection;
using UNI.API.Client;
using UNI.Core.Library;

namespace UNI.Core.UI.CustomControls
{
    public class DataGridBaseModelColumn<T> : DataGridComboBoxColumn where T : BaseModel
    {
        readonly UNIClient<T> BaseClient;
        private readonly string dependencyFilterPropertyName;
        private readonly string parentFilterPropertyName;
        private readonly string depenencyFilterPropertyValue;

        public DataGridBaseModelColumn(string dependencyFilterPropertyName, string parentFilterPropertyName, string depenencyFilterPropertyValue)
        {
            BaseClient = new UNIClient<T>();

            this.dependencyFilterPropertyName = dependencyFilterPropertyName;
            this.parentFilterPropertyName = parentFilterPropertyName;
            this.depenencyFilterPropertyValue = depenencyFilterPropertyValue;

            DisplayMemberPath = GetDisplayPropertyInfo();
        }
        private string GetDisplayPropertyInfo()
        {
            foreach (var pinfo in typeof(T).GetProperties())
                if (pinfo.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                    if (valueInfo.IsDisplayProperty)
                        return pinfo.Name;

            return typeof(T).GetProperties()[0].Name;
        }

        // TODO cosa è questo (zero references?)
        public async void LoadData(BaseModel parent = null)
        {
            if (!string.IsNullOrWhiteSpace(dependencyFilterPropertyName) && !string.IsNullOrWhiteSpace(parentFilterPropertyName) && parent != null)
            {
                var property = parent.GetType().GetProperty(parentFilterPropertyName);
                if (property != null)
                {
                    int parentPropertyId = (int)property.GetValue(parent, null);
                    if (parentPropertyId != 0)
                        ItemsSource = await BaseClient.GetItemsWhere(parentPropertyId, dependencyFilterPropertyName) ?? new List<T>();
                    else
                        ItemsSource = await BaseClient.GetItems() ?? new List<T>();
                }
            }
            else if (!string.IsNullOrWhiteSpace(dependencyFilterPropertyName) && !string.IsNullOrWhiteSpace(depenencyFilterPropertyValue))
            {
                List<T> itemsSource = await BaseClient.GetItems() ?? new List<T>();
                var filteredSource = new List<T>();
                foreach (T item in itemsSource)
                {
                    var property = typeof(T).GetProperty(dependencyFilterPropertyName);
                    if (property != null)
                    {
                        var value = property.GetValue(item);
                        if (value != null)
                        {
                            if (value.ToString().ToLower().Equals(depenencyFilterPropertyValue.ToLower()))
                            {
                                filteredSource.Add(item);
                            }
                        }
                    }
                }
                itemsSource = filteredSource;
                ItemsSource = itemsSource;
            }
            else
                ItemsSource = await BaseClient.GetItems() ?? new List<T>();
        }
    }
}
