using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UNI.Core.Library;
using UNI.Core.Library.Mapping;
using UNI.Core.UI.Converters;
using UNI.Core.UI.CustomControls;
using UNI.Core.UI.Misc;
using UNI.Core.UI.Services.Printing;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace UNI.Core.UI.Tabs.ListGrid
{
    public class ListGridVB<T> : ViewBuilder.ViewBuilder<T> where T : BaseModel
    {
        protected const byte PrintedFontSize = 10;

        #region Event: RowEdit

        internal event EventHandler<DataGridRowEditEndingEventArgs> RowEditEnding;
        internal void Datagrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e) => RowEditEnding?.Invoke(sender, e);

        #endregion

        #region Public methods

        /// <summary>
        /// Builds the main DataGrid to display in the ListGrid view
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <param name="pageGroup"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public override FrameworkElement RenderMainFrameworkElement(BaseModel selectedItem = null, string pageGroup = null, int? height = null)
        {
            var datagrid = BuildXAMLDatagrid(height);
            SetItemsSourceBinding(datagrid);
            return datagrid;
        }

        /// <summary>
        /// Builds a printer-friendly version of the Datagrid
        /// </summary>
        /// <param name="itemsSource"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public List<FrameworkElement> RenderDataGridForPrinting(System.Collections.ObjectModel.ObservableCollection<T> itemsSource, int? height = null)
        {
            var datagrids = new List<FrameworkElement>();
            var subLists = ChunkHelper.Chunkify(itemsSource, PrintBuilder.EntriesPerPage);
            foreach (var subList in subLists)
            {
                var datagrid = BuildXAMLDatagrid(height, isForPrinting: true);
                (datagrid as DataGrid).ItemsSource = subList;
                datagrids.Add(datagrid);
            }
            return datagrids;
        }

        #endregion

        #region Private methods

        void SetItemsSourceBinding(FrameworkElement datagrid)
        {
            var bindingItemsSource = new Binding
            {
                Path = new PropertyPath("ItemsSource"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            datagrid.SetBinding(DataGrid.ItemsSourceProperty, bindingItemsSource);
        }

        FrameworkElement BuildXAMLDatagrid(int? height = null, bool isForPrinting = false)
        {
            var datagrid = new DataGrid();

            List<PropertyInfo> propertyInfos = typeof(T).GetProperties().ToList();

            if (VisibleProperties.Any() && propertyInfos.Any())
            {
                foreach (string visibileProperty in VisibleProperties)
                {
                    PropertyInfo property = propertyInfos.Find(p => p.Name == visibileProperty);

                    if (visibileProperty.Contains("."))
                        property = propertyInfos.Find(p => p.Name == visibileProperty.Split('.')[0]);

                    if (property == null)
                        continue;

                    var valueInfo = (ValueInfo)property.GetCustomAttribute(typeof(ValueInfo));
                    if (valueInfo == null)
                        continue;

                    switch (property.PropertyType)
                    {
                        case Type boolType when boolType == typeof(bool):
                            if (isForPrinting)
                                BuildPrintAdaptedCheckBoxColumn(datagrid, property, valueInfo);
                            else
                                BuildCheckBoxColumn(datagrid, property, valueInfo);
                            break;

                        case Type baseModelType when baseModelType.IsSubclassOf(typeof(BaseModel)):
                            BuildBaseModelColumn(datagrid, property, valueInfo, isForPrinting);
                            break;

                        default:
                            BuildDefaultColumn(datagrid, property, valueInfo, isForPrinting);
                            break;
                    }
                }
            }
            else
            {
                foreach (var property in propertyInfos)
                {
                    var binding = GetStandardBinding(property);

                    var renderInfo = property.GetCustomAttribute(typeof(RenderInfo)) as RenderInfo;
                    if (renderInfo?.Converter != null)
                        binding.Converter = (IValueConverter)Activator.CreateInstance(PlatformTypeConverterHelper.GetSpecificValueConverter(renderInfo.Converter));

                    var column = new DataGridTextColumn
                    {
                        Header = property.Name,
                        IsReadOnly = false,
                        Binding = binding,
                    };

                    if (isForPrinting)
                        column.FontSize = PrintedFontSize;

                    datagrid.Columns.Add(column);
                }
            }

            var bindingSelectedItem = new Binding
            {
                Path = new PropertyPath("SelectedItem"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            datagrid.RowEditEnding += Datagrid_RowEditEnding;
            datagrid.AutoGenerateColumns = false;
            datagrid.IsReadOnly = false;

            datagrid.SetBinding(DataGrid.SelectedItemProperty, bindingSelectedItem);
            datagrid.BorderBrush = Application.Current.Resources["ButtonDisabledBorderThemeBrush"] as Windows.UI.Xaml.Media.SolidColorBrush;
            datagrid.BorderThickness = new Thickness(1);

            if (height.HasValue)
                datagrid.Height = height ?? 500;

            return datagrid;

        }

        void BuildCheckBoxColumn(DataGrid datagrid, PropertyInfo property, ValueInfo valueInfo)
        {
            var checkBoxColumn = new DataGridCheckBoxColumn { Header = ResourceLoader.GetString($"textbox_{typeof(T).Name}_{property.Name}") };
            if (string.IsNullOrWhiteSpace(checkBoxColumn.Header.ToString()))
                checkBoxColumn.Header = property.Name;

            checkBoxColumn.IsReadOnly = valueInfo.IsReadOnly;
            checkBoxColumn.Binding = GetStandardBinding(property);
            datagrid.Columns.Add(checkBoxColumn);
        }

        void BuildPrintAdaptedCheckBoxColumn(DataGrid datagrid, PropertyInfo property, ValueInfo valueInfo)
        {
            var column = new DataGridTextColumn { Header = ResourceLoader.GetString($"textbox_{typeof(T).Name}_{property.Name}") };

            if (string.IsNullOrWhiteSpace(column.Header.ToString()))
                column.Header = property.Name;

            column.FontSize = PrintedFontSize;

            var binding = GetStandardBinding(property);
            binding.Converter = new BoolToStringConverter();

            column.Binding = binding;
            datagrid.Columns.Add(column);
        }

        void BuildBaseModelColumn(DataGrid datagrid, PropertyInfo property, ValueInfo valueInfo, bool isForPrinting = false)
        {
            DataGridBoundColumn column;

            if (property.GetCustomAttribute(typeof(RenderInfo)) is RenderInfo renderInfo && renderInfo.IsDataGridEditable)
            {
                Type classType = typeof(DataGridBaseModelColumn<>);
                Type[] typeParams = new Type[] { property.PropertyType };
                Type constructedType = classType.MakeGenericType(typeParams);

                column = (DataGridComboBoxColumn)Activator.CreateInstance(constructedType);

                if (isForPrinting)
                    (column as DataGridComboBoxColumn).FontSize = PrintedFontSize;
            }
            else
            {
                column = new DataGridTextColumn();

                if (isForPrinting)
                    (column as DataGridTextColumn).FontSize = PrintedFontSize;
            }

            string header = ResourceLoader.GetString($"textbox_{typeof(T).Name}_{property.Name}");
            if (string.IsNullOrWhiteSpace(header))
                column.Header = string.IsNullOrWhiteSpace(header) ? property.Name : header;

            column.IsReadOnly = valueInfo.IsReadOnly;
            column.Binding = GetStandardBinding(property);
            datagrid.Columns.Add(column);
        }

        void BuildDefaultColumn(DataGrid datagrid, PropertyInfo property, ValueInfo valueInfo, bool isForPrinting = false)
        {
            var binding = GetStandardBinding(property);

            var renderInfo = property.GetCustomAttribute(typeof(RenderInfo)) as RenderInfo;

            if (renderInfo?.Converter != null)
                binding.Converter = (IValueConverter)Activator.CreateInstance(PlatformTypeConverterHelper.GetSpecificValueConverter(renderInfo.Converter));

            if (renderInfo != null && renderInfo.IsFixedValue)
            {
                var columnComboBox = new DataGridComboBoxColumn { Header = ResourceLoader.GetString($"textbox_{typeof(T).Name}_{property.Name}") };
                string itemsstring = ResourceLoader.GetString($"combobox_{typeof(T).Name}_{property.Name}") ?? string.Empty;
                columnComboBox.ItemsSource = itemsstring.Split(',').ToList();
                columnComboBox.IsReadOnly = valueInfo.IsReadOnly;
                columnComboBox.Binding = binding;
                datagrid.Columns.Add(columnComboBox);
            }
            else
            {
                var column = new DataGridTextColumn { Header = ResourceLoader.GetString($"textbox_{typeof(T).Name}_{property.Name}") };

                if (string.IsNullOrWhiteSpace(column.Header.ToString()))
                    column.Header = property.Name;

                if (isForPrinting)
                    column.FontSize = PrintedFontSize;

                column.IsReadOnly = valueInfo.IsReadOnly;
                column.Binding = binding;
                datagrid.Columns.Add(column);
            }
        }

        Binding GetStandardBinding(PropertyInfo property)
        {
            return new Binding
            {
                Path = new PropertyPath($"{property.Name}"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = property.PropertyType == typeof(DateTime) ? new DateTimeToStringLocalTime() : null
            };
        }

        #endregion
    }
}