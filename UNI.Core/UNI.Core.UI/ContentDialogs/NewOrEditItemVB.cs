using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using UNI.Core.Library;
using UNI.Core.UI.CustomControls.GridBox;
using UNI.Core.UI.CustomControls.PropertiesGroup;
using UNI.Core.UI.CustomControls.ShowBox;
using UNI.Core.UI.CustomControls.UniTextBox;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

namespace UNI.Core.UI.ContentDialogs
{
    internal class NewOrEditItemVB<T> : ViewBuilder.ViewBuilder<T> where T : BaseModel
    {
        public override FrameworkElement RenderMainFrameworkElement(BaseModel selectedItem, string pageGroup = null, int? height = null)
        {
            string baseitem = ResourceLoader.GetString($"baseModel_{typeof(T).Name}");
            if (string.IsNullOrWhiteSpace(baseitem))
                baseitem = typeof(T).Name;

            if (pageGroup == baseitem)
                pageGroup = null;

            // iterate through properties to render control and assign it to container
            var controls = GetPropertyControl(selectedItem, pageGroup);

            // create container for the controls and the first row
            Grid grid = new Grid { ColumnSpacing = 20, RowSpacing = 20 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var allowedTypes = new List<Type>
                {
                    typeof(PropertiesGroup),
                    typeof(ComboBox),
                    typeof(ShowBox),
                    typeof(UniTextBox),
                    typeof(GridBox),
                    typeof(DatePicker),
                    typeof(Line),
                    typeof(DataGrid),
                };

            int rowIndex = 0;

            foreach (var control in controls.Where(c => allowedTypes.Contains(c.GetType())))
            {
                if (control.GetType().Equals(typeof(PropertiesGroup)))
                {
                    var propertiesGroup = control as PropertiesGroup;
                    foreach (var subControl in propertiesGroup.Controls.Where(c => allowedTypes.Contains(c.GetType())))
                    {

                        Grid.SetRow(subControl, rowIndex);
                        grid.Children.Add(subControl);

                        // and set the cursor to next row, which we are going to add
                        rowIndex++;
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }
                }
                else
                {
                    Grid.SetRow(control, rowIndex);
                    grid.Children.Add(control);

                    // and set the cursor to next row, which we are going to add
                    rowIndex++;
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
            }
            return grid;
        }
    }
}