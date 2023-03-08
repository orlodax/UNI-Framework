using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UNI.Core.Library;
using UNI.Core.Library.Mapping;
using UNI.Core.UI.CustomControls.GridBox;
using UNI.Core.UI.CustomControls.PropertiesGroup;
using UNI.Core.UI.CustomControls.UniTextBox;
using UNI.Core.UI.CustomEventArgs;
using UNI.Core.UI.Tabs;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

namespace UNI.Core.UI.ViewBuilder
{
    public abstract class ViewBuilder<T> where T : BaseModel
    {
        #region Events 
        /// <summary>
        /// TODO commento
        /// </summary>
        public event EventHandler<ItemUpdatedEventArgs> ItemUpdated;
        private void ControlVM_ItemUpdated(object sender, ItemUpdatedEventArgs e) => ItemUpdated?.Invoke(sender, e);



        /// <summary>
        /// Called by UniTextBox, observed in BaseTab
        /// </summary>
        public virtual event EventHandler<RoutedEventArgs> LostFocus;
        private void OnLostFocus(object sender, RoutedEventArgs e) => LostFocus?.Invoke(sender, e);
        #endregion

        #region Properties/fields
        /// <summary>
        /// For all inheriting VBs
        /// </summary>
        protected ResourceLoader ResourceLoader = ResourceLoader.GetForCurrentView();

        /// <summary>
        /// Object holding all customizations (and default values) for this ViewBuilder/FinalVM instance
        /// </summary>
        private readonly ViewModelResolver viewModelResolver = new ViewModelResolver();
        #endregion

        #region Public Surface
        /// <summary>
        /// Called optionally in the final VM to replace default VMs of Custom Controls with a BaseViewModel datacontext
        /// </summary>
        /// <param name="control">The FrameWorkElement type that is going to be rendered</param>
        /// <param name="vm">The ViewModel type to assign to the view</param>
        public void CustomizeViewModel(EnControlTypes control, Type vm)
        {
            viewModelResolver.CustomizeViewModel(control, vm);
        }
        public void CustomizeViewModelByPropertyName(string propertyName, Type vm)
        {
            viewModelResolver.CustomizeViewModelByPropertyName(propertyName, vm);
        }
        public void CustomizeNewItemVM(EnControlTypes control, Type newItemVMType)
        {
            viewModelResolver.CustomizeNewItemVM(control, newItemVMType);
        }
        public void CustomizeEditItemVM(EnControlTypes control, Type editItemVMType)
        {
            viewModelResolver.CustomizeEditItemVM(control, editItemVMType);
        }
        public void CustomizeShowBoxVM(EnControlTypes control, Type showBoxVMType)
        {
            viewModelResolver.CustomizeShowBoxVM(control, showBoxVMType);
        }

        /// <summary>
        /// Populated in the final customized VB, to drive below method in selectively building and displaying controls
        /// </summary>
        protected List<string> VisibleProperties = new List<string>();
        /// <summary>
        /// No need to extend VB to write to properties; just call this from the final VM 
        /// </summary>
        /// <param name="visibleProperties"></param>
        public void CustomizeVisibleProperties(List<string> visibleProperties)
        {
            VisibleProperties = visibleProperties;
        }
        #endregion

        #region Shared Methods
        /// <summary>
        /// Derived VBs will implement this method to create the main element in the view, which includes calling the below: GetPropertyControl(BaseModel selectedItem, string pageGroup = null)
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <param name="pageGroup"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public abstract FrameworkElement RenderMainFrameworkElement(BaseModel selectedItem = null, string pageGroup = null, int? height = null);

        /// <summary>
        /// Looks at the types of the properties of the selected BaseModel and call factory.build for matching control type
        /// </summary>
        protected List<FrameworkElement> GetPropertyControl(BaseModel selectedItem, string pageGroup = null)
        {
            var controlsFactory = new ControlsFactory(viewModelResolver);

            selectedItem.InitModel();

            if (pageGroup != null)
                foreach (var p in typeof(T).GetProperties())
                    if (p.GetCustomAttribute(typeof(RenderInfo)) is RenderInfo renderInfo && !string.IsNullOrWhiteSpace(renderInfo.PageGroup))
                    {
                        string item = ResourceLoader.GetString($"group_{typeof(T).Name}_{renderInfo.PageGroup}");
                        if (string.IsNullOrWhiteSpace(item))
                            item = renderInfo.PageGroup;

                        if (pageGroup == item)
                            pageGroup = renderInfo.PageGroup;
                    }

            // find all properties groups
            var propertiesGroups = new List<PropertiesGroup>();
            foreach (var p in selectedItem.GetType().GetProperties())
                if (p.GetCustomAttribute(typeof(RenderInfo)) is RenderInfo renderInfo && !string.IsNullOrWhiteSpace(renderInfo.Group))
                    if (propertiesGroups.Find(g => g.Name == renderInfo.Group) == null)
                        propertiesGroups.Add(new PropertiesGroup(renderInfo.Group, ResourceLoader.GetString($"textbox_{typeof(T).Name}_{renderInfo.Group}") ?? renderInfo.Group));

            // collect only the visible properties associated with this basemodel final type
            var properties = new List<PropertyInfo>();
            List<PropertyInfo> allProperties = selectedItem.GetType().GetProperties().ToList();
            if (VisibleProperties.Any())
                properties.AddRange(allProperties.Where(p => VisibleProperties.Any(v => v == p.Name)));
            else
                properties = allProperties;

            var controls = new List<FrameworkElement>();
            foreach (var property in properties)
            {
                if (property != null)
                {
                    FrameworkElement control = controlsFactory.BuildControl(property, typeof(T).Name, selectedItem);
                    if (control == null)
                        continue;

                    // whatever it is, if the control has a VM, register for item update event and navigation to tab event
                    if (control.DataContext is BaseTabVMTypeAgnostic baseVM)
                    {
                        baseVM.ItemUpdated += ControlVM_ItemUpdated;
                    }

                    if (control is UniTextBox utb)
                        utb.LostFocus += OnLostFocus;

                    if (!(property.GetCustomAttribute(typeof(RenderInfo)) is RenderInfo renderInfo))
                    {
                        controls.Add(control);
                    }
                    else
                    {
                        if (renderInfo.PageGroup == pageGroup || (string.IsNullOrEmpty(renderInfo.PageGroup) && pageGroup == null)) //either same or both null
                        {
                            if (string.IsNullOrWhiteSpace(renderInfo.Group))
                            {
                                //if the property must start on a new line insert in the control list a void Line, the specific viewbuilder will manage it inserting a new row
                                if (renderInfo.NewLine)
                                    controls.Add(new Line());

                                controls.Add(control);
                            }
                            else
                            {
                                PropertiesGroup propertyGroup = propertiesGroups.Find(g => g.Name == renderInfo.Group);

                                if (renderInfo.NewLine)
                                    propertyGroup.Controls.Add(new Line());

                                propertyGroup.Controls.Add(control);
                            }
                        }
                    }
                }
            }

            // now also add the properties group. Their internal list of controls was populated in the previous foreach
            foreach (PropertiesGroup pg in propertiesGroups)
                if (pg.Controls.Any())
                    controls.Add(pg);

            return controls;
        }

        internal FrameworkElement RenderGrid(List<FrameworkElement> controls, bool isListDetail = false) // three(hundred) is a magic number...  
        {
            // window - nav menu
            var detailsWidth = Window.Current.Bounds.Width - 300;

            // if called by listdetail, there's less space available (the list on the left in addition to the navigation view)
            if (isListDetail)
                detailsWidth -= 300;

            // single control width should span from 250 to 350px
            int averageControlWidth = 350;
            var numberOfColumns = Convert.ToInt32(Math.Round(detailsWidth / averageControlWidth));
            if (numberOfColumns == 0)
                numberOfColumns = 1;

            // create container for the controls, the evaluated columns and the first row
            Grid grid = new Grid { ColumnSpacing = 15, RowSpacing = 10 };
            for (int i = 0; i < numberOfColumns; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // assign the controls to the columns
            int colIndex = 0;
            int rowIndex = 0;

            var fullWidthControlTypes = new List<Type>()
            {
                typeof(GridBox),
                typeof(PropertiesGroup),
                typeof(Line),
                typeof(DataGrid)
            };

            // add a row with the navigation view
            foreach (var control in controls)
            {
                if (control.GetType() != typeof(NavigationViewItem))
                {
                    // if the control is a little datagrid or other full-widht control, it should take the whole row 
                    if (fullWidthControlTypes.Contains(control.GetType()))
                    {
                        // and set the cursor to first column, next row, which we are going to add
                        colIndex = 0;
                        rowIndex++;
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                        Grid.SetColumnSpan(control, numberOfColumns);
                        Grid.SetColumn(control, colIndex);
                        Grid.SetRow(control, rowIndex);
                        grid.Children.Add(control);

                        // and again a new row
                        rowIndex++;
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }
                    else
                    {
                        // if we reached last column, reset column cursor and add a row
                        if (colIndex == numberOfColumns)
                        {
                            colIndex = 0;
                            rowIndex++;
                            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        }

                        // Assign current control to its position in the grid. Warning: if control for some reason is already assigned to somewhere, this will crash badly on grid.Children.Add(control);
                        Grid.SetColumn(control, colIndex);
                        Grid.SetRow(control, rowIndex);
                        grid.Children.Add(control);

                        colIndex++;
                    }
                }
            }

            // this is a trick that resolve a very annoying problem. If i return directly the grid every time that i lost focus on a control, the focus return on
            // the first item. We have to investigate to solve definitely this problem
            Grid mainGrid = new Grid { ColumnSpacing = 20, RowSpacing = 20 };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            NavigationView navigationView = new NavigationView() { Height = 0 };
            Grid.SetRow(navigationView, 0);
            mainGrid.Children.Add(navigationView);

            Grid.SetRow(grid, 1);
            mainGrid.Children.Add(grid);
            return mainGrid;
        }

        internal List<FrameworkElement> GetControlsForPrinting(BaseModel selectedItem)
        {
            var controls = new List<FrameworkElement>();

            // get all controls like normal
            controls.AddRange(GetPropertyControl(selectedItem));

            // add back controls that are nested in PropertiesGroups
            var propertiesGroups = controls.Where(c => c.GetType() == typeof(PropertiesGroup)).Cast<PropertiesGroup>();
            controls.AddRange(propertiesGroups.SelectMany(pg => pg.Controls));

            // remove the propertiesGroups
            controls = controls.Where(c => !propertiesGroups.Contains(c)).ToList();

            // assign values to unbound controls
            foreach (var ts in controls.Where(c => c.GetType() == typeof(ToggleSwitch)).Select(c => (ToggleSwitch)c))
            {
                var propertyValue = selectedItem.GetType().GetProperties()
                    .First(p => p.Name.ToLower() == ts.Header.ToString().ToLower())
                    .GetValue(selectedItem);

                ts.IsOn = propertyValue != null && (bool)propertyValue;
            }
            foreach (var utb in controls.Where(c => c.GetType() == typeof(UniTextBox)).Select(c => (UniTextBox)c))
            {
                var propertyValue = utb.PropertyInfo.GetValue(selectedItem);

                utb.TextBox.Text = Convert.ToString(propertyValue);
            }
            foreach (var picker in controls.Where(c => c.GetType() == typeof(DatePicker)).Select(c => (DatePicker)c))
            {
                var propertyValue = selectedItem.GetType().GetProperties()
                    .First(p => p.Name.ToLower() == picker.Header.ToString().ToLower())
                    .GetValue(selectedItem);

                picker.SelectedDate = Convert.ToDateTime(propertyValue);
            }

            foreach (var gb in controls.Where(c => c.GetType() == typeof(GridBox)).Select(c => (GridBox)c))
            {
                var cellStyle = new Style(typeof(DataGridCell));
                cellStyle.Setters.Add(new Setter(Control.FontSizeProperty, 10));

                ((gb.DataContext as dynamic).MainGrid as DataGrid).CellStyle = cellStyle;
            }

            return controls;
        }
        #endregion
    }
}