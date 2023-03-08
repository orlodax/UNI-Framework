using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UNI.Core.Library.Mapping;
using UNI.Core.UI.Converters;
using UNI.Core.UI.CustomControls.GridBox;
using UNI.Core.UI.CustomControls.ShowBox;
using UNI.Core.UI.CustomControls.UniTextBox;
using UNI.Core.UI.Misc;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace UNI.Core.UI.Services.Printing
{
    public class PrintBuilder
    {
        public static byte EntriesPerPage = 29;
        private const int _XpixelsA4 = 732;
        private const byte _numberOfColumns = 3;

        private readonly object _sender;

        public PrintBuilder(object sender)
        {
            _sender = sender;
        }

        internal IEnumerable<FrameworkElement> BuildPages(IEnumerable<FrameworkElement> controls)
        {
            var pages = new List<FrameworkElement>();
            byte pageNumber = 1;

            var simpleControlsChunks = ChunkHelper.ChunkifyAll(controls, (EntriesPerPage + 1) * _numberOfColumns);

            foreach (var chunk in simpleControlsChunks)
            {
                pages.Add(BuildPage(chunk, pageNumber));
                pageNumber++;
            }

            return pages;
        }

        private FrameworkElement BuildPage(IEnumerable<FrameworkElement> controls, byte pageNumber, bool isControlDatagrid = false)
        {
            var grid = new Grid();

            // first column for header, second for content, third for footer
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            if (pageNumber == 1)
                AddHeaderToFirstPage(grid);

            var subGrid = BuildPageBody(controls);
            Grid.SetRow(subGrid, 1);
            grid.Children.Add(subGrid);

            AddFooterToPage(grid, pageNumber);

            return grid;
        }

        // if first page add title
        private void AddHeaderToFirstPage(Grid grid)
        {
            string title = "UNI Print";
            Type baseType = _sender.GetType().BaseType;

            if (baseType.GetGenericArguments().Any())
            {
                var baseModelName = ResourceLoader.GetForCurrentView().GetString($"baseModel_{baseType.GetGenericArguments()[0].Name}");
                if (!string.IsNullOrWhiteSpace(baseModelName))
                    title = baseModelName;
            }

            var header = new TextBlock { Text = title, FontSize = 16, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 20) };
            Grid.SetRow(header, 0);
            grid.Children.Add(header);
        }

        private void AddFooterToPage(Grid grid, byte pageNumber)
        {
            var footer = new TextBlock { Text = $"page {pageNumber}", Margin = new Thickness(0, 20, 0, 0) };
            Grid.SetRow(footer, 2);
            grid.Children.Add(footer);
        }

        private Grid BuildPageBody(IEnumerable<FrameworkElement> controls)
        {
            var subGrid = new Grid() { Padding = new Thickness(10), Height = Double.NaN };
            for (byte i = 0; i < _numberOfColumns; i++)
                subGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(_XpixelsA4 / 3, GridUnitType.Pixel) });

            subGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // assign the controls to the columns
            byte colIndex = 0;
            byte rowIndex = 0;

            foreach (var control in controls)
            {
                if (control.GetType() == typeof(GridBox))
                    GetPrintedGridBox(control, ref colIndex, ref rowIndex, subGrid);
                else
                    GetSinglePrintedField(control, ref colIndex, ref rowIndex, subGrid);
            }

            return subGrid;
        }

        private void GetSinglePrintedField(FrameworkElement control, ref byte colIndex, ref byte rowIndex, Grid subGrid)
        {
            var adaptedControl = new PrintedField();

            switch (control)
            {
                case ShowBox sb:
                    var vm = sb.DataContext as dynamic;
                    adaptedControl.FieldName = Convert.ToString(vm.MemberName);
                    adaptedControl.FieldValue = Convert.ToString(vm.DisplayProperty);
                    break;

                case ComboBox cb:
                    adaptedControl.FieldName = Convert.ToString(cb.Header);
                    adaptedControl.FieldValue = Convert.ToString(cb.SelectedValue);
                    break;

                case ToggleSwitch ts:
                    adaptedControl.FieldName = Convert.ToString(ts.Header);
                    adaptedControl.FieldValue = new BoolToStringConverter().Convert(ts.IsOn, null, null, null).ToString();
                    break;

                case TextBox tb:
                    adaptedControl.FieldName = Convert.ToString(tb.Header);
                    adaptedControl.FieldValue = tb.Text;
                    break;

                case UniTextBox tb:
                    string value = Convert.ToString(tb.TextBox.Text);
                    if (tb.PropertyInfo.GetCustomAttribute(typeof(RenderInfo)) is RenderInfo renderInfo && renderInfo.Converter != null)
                        if (double.TryParse(value, out double doubleValue))
                            value = (Activator.CreateInstance(PlatformTypeConverterHelper.GetSpecificValueConverter(renderInfo.Converter)) as IValueConverter)
                                .Convert(doubleValue, null, null, null).ToString();

                    adaptedControl.FieldName = Convert.ToString(tb.TextBox.Header);
                    adaptedControl.FieldValue = value;
                    break;

                case DatePicker dp:
                    adaptedControl.FieldName = Convert.ToString(dp.Header);
                    adaptedControl.FieldValue = dp.SelectedDate.GetValueOrDefault().ToString("dd MMM yyyy");
                    break;

                case Line line:
                    colIndex = 0;
                    rowIndex++;
                    subGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    break;

                default:
                    adaptedControl = null;
                    break;
            }

            if (adaptedControl == null)
                return;

            if (string.IsNullOrWhiteSpace(adaptedControl.FieldName) && string.IsNullOrWhiteSpace(adaptedControl.FieldValue))
                return;

            // if we reached last column, reset column cursor and add a row
            if (colIndex == _numberOfColumns)
            {
                colIndex = 0;
                rowIndex++;
                subGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Assign current control to its position in the grid. Warning: if control for some reason is already assigned to somewhere, this will crash badly on grid.Children.Add(control);
            Grid.SetColumn(adaptedControl, colIndex);
            Grid.SetRow(adaptedControl, rowIndex);
            subGrid.Children.Add(adaptedControl);
            colIndex++;
        }

        private void GetPrintedGridBox(FrameworkElement control, ref byte colIndex, ref byte rowIndex, Grid subGrid)
        {
            rowIndex++;
            subGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            dynamic destinationControl = (control.DataContext as dynamic).MainGrid;
            destinationControl.Height = double.NaN;
            destinationControl.BorderBrush = new SolidColorBrush(Colors.Black);
            destinationControl.GridLinesVisibility = DataGridGridLinesVisibility.All;
            destinationControl.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
            destinationControl.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);

            subGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var gridBoxName = new TextBlock()
            {
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = (control.DataContext as dynamic).GridBoxName
            };
            destinationControl.DataContext = control.DataContext;

            Grid.SetRow(gridBoxName, rowIndex);
            rowIndex++;
            subGrid.Children.Add(gridBoxName);

            colIndex = 0;
            Grid.SetRow(destinationControl, rowIndex);
            Grid.SetColumnSpan(destinationControl, _numberOfColumns);
            Grid.SetColumn(destinationControl, colIndex);
            subGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            rowIndex++;
            subGrid.Children.Add(destinationControl);
        }
    }
}
