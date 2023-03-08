using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.Library.Mapping;
using UNI.Core.UI.CustomControls;
using UNI.Core.UI.CustomControls.GridBox;
using UNI.Core.UI.CustomControls.ImageBox;
using UNI.Core.UI.CustomControls.ShowBox;
using UNI.Core.UI.CustomControls.UniTextBox;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace UNI.Core.UI.ViewBuilder
{
    internal class ControlsFactory
    {
        private readonly ViewModelResolver ViewModelResolver;
        private readonly ResourceLoader ResourceLoader = ResourceLoader.GetForCurrentView();

        internal ControlsFactory(ViewModelResolver viewModelResolver)
        {
            ViewModelResolver = viewModelResolver;
        }

        /// <summary>
        /// Factory method called by the viewbuilder
        /// </summary>
        internal FrameworkElement BuildControl(PropertyInfo prop, string baseModelTypeName, BaseModel selectedItem)
        {
            FrameworkElement control = null;
            EnControlTypes controlType = GetControlType(prop);
            switch (controlType)
            {
                case EnControlTypes.ComboBox:
                    return CreateComboBox(prop, baseModelTypeName);

                case EnControlTypes.ToggleSwitch:
                    return CreateToggleSwitch(prop, baseModelTypeName);

                case EnControlTypes.TextBox:
                    return CreateDateTextBox(prop, baseModelTypeName);

                case EnControlTypes.DatePicker:
                    return CreateDatePicker(prop, baseModelTypeName);

                case EnControlTypes.UniTextBox:
                    return CreateUniTextBox(prop, baseModelTypeName);

                case EnControlTypes.ImageBox:
                    return CreateImageBox(prop, baseModelTypeName, selectedItem);

                case EnControlTypes.DocumentBox:
                    return CreateDocumentBox(prop, baseModelTypeName, selectedItem);

                case EnControlTypes.GridBox:
                    return CreateGridBox(prop, baseModelTypeName, selectedItem, ViewModelResolver.GetVMType(EnControlTypes.GridBox, prop.Name));

                case EnControlTypes.GridBoxDataSet:
                    return CreateGridBoxDataSet(prop, baseModelTypeName, selectedItem, ViewModelResolver.GetVMType(EnControlTypes.GridBoxDataSet, prop.Name));

                case EnControlTypes.GridBoxView:
                    return CreateGridBoxView(prop, baseModelTypeName, selectedItem, ViewModelResolver.GetVMType(EnControlTypes.GridBoxView, prop.Name));

                case EnControlTypes.GridBoxMtM:
                    return CreateGridBoxMtM(prop, baseModelTypeName, selectedItem, ViewModelResolver.GetVMType(EnControlTypes.GridBoxMtM, prop.Name));

                case EnControlTypes.ShowBox:
                    return CreateShowBox(prop, baseModelTypeName, selectedItem, ViewModelResolver.GetShowBoxVMType(EnControlTypes.ShowBox));

                default:
                    break;
            }
            return control;
        }

        private EnControlTypes GetControlType(PropertyInfo property)
        {
            var valueInfo = property.GetCustomAttribute(typeof(ValueInfo)) as ValueInfo;
            var renderInfo = property.GetCustomAttribute(typeof(RenderInfo)) as RenderInfo;

            Type type = property.PropertyType;

            if (type.FullName.Contains("UniDataSet"))
            {
                return EnControlTypes.GridBoxDataSet;
            }
            else if (property.PropertyType.IsSubclassOf(typeof(BaseModel)))
            {
                if (type == typeof(UniImage))
                    return EnControlTypes.ImageBox;

                else if (type == typeof(Document))
                    return EnControlTypes.DocumentBox;

                else
                    return EnControlTypes.ShowBox;
            }
            else if (type.IsGenericType)
            {
                bool hasBaseModelAncestor = type.GetGenericArguments()[0].IsSubclassOf(typeof(BaseModel));

                if (string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                {
                    if (hasBaseModelAncestor)
                        return EnControlTypes.GridBox;
                    else
                        return EnControlTypes.GridBoxView;
                }
                else
                {
                    if (hasBaseModelAncestor)
                        return EnControlTypes.GridBoxMtM;
                }
            }
            else if (valueInfo != null)
            {
                if (valueInfo.IsVisible)
                {
                    if (renderInfo?.IsFixedValue ?? false)
                    {
                        return EnControlTypes.ComboBox;
                    }
                    else if (type == typeof(bool))
                    {
                        return EnControlTypes.ToggleSwitch;
                    }
                    else if (type == typeof(DateTime))
                    {
                        if (property.Name == "Created" || property.Name == "LastModify")
                            return EnControlTypes.TextBox;
                        else
                            return EnControlTypes.DatePicker;
                    }
                    else if (type.IsValueType || type == typeof(string))
                    {
                        return EnControlTypes.UniTextBox;
                    }
                }
            }

            return EnControlTypes.Null;
        }

        #region ComboBox
        ComboBox CreateComboBox(PropertyInfo property, string baseModelTypeName)
        {
            var cb = new ComboBox { Style = (Style)Application.Current.Resources["BaseComboBox"] };
            string itemsstring = ResourceLoader.GetString($"combobox_{baseModelTypeName}_{property.Name}") ?? string.Empty;
            cb.ItemsSource = itemsstring.Split(',').ToList();

            Binding bindingSelectedItem = new Binding
            {
                Path = new PropertyPath($"SelectedItem.{property.Name}"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            cb.SetBinding(ComboBox.SelectedItemProperty, bindingSelectedItem);
            cb.Header = ResourceLoader.GetString($"textbox_{baseModelTypeName}_{property.Name}");

            // just to see something if we haven't updated /Strings/languages...
            if (string.IsNullOrWhiteSpace(cb.Header.ToString()))
                if (property.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo attrib)
                    cb.Header = attrib.SQLName;

            return cb;
        }
        #endregion

        #region DatePicker
        DatePicker CreateDatePicker(PropertyInfo property, string baseModelTypeName)
        {
            var dp = new DatePicker
            {
                Header = ResourceLoader.GetString($"textbox_{baseModelTypeName}_{property.Name}"),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 5, 0, 0),
                MinHeight = 32
            };

            var bindingDate = new Binding
            {
                Path = new PropertyPath($"SelectedItem.{property.Name}"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new Converters.DateTimeToDateTimeOffsetConverter()
            };
            dp.SetBinding(DatePicker.DateProperty, bindingDate);

            // just to see something if we haven't updated /Strings/languages...
            if (string.IsNullOrWhiteSpace(dp.Header.ToString()))
                if (property.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo attrib)
                    dp.Header = attrib.SQLName;

            return dp;
        }
        #endregion

        #region (Date)TextBox
        TextBox CreateDateTextBox(PropertyInfo property, string baseModelTypeName)
        {
            var tb = new TextBox
            {
                Style = (Style)Application.Current.Resources["BaseTextBox"],
                IsReadOnly = true,
                Header = ResourceLoader.GetString($"textbox_{baseModelTypeName}_{property.Name}")
            };

            Binding bindingText = new Binding
            {
                Path = new PropertyPath($"SelectedItem.{property.Name}"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new Converters.DateToStringNiceConverter(),
            };
            tb.SetBinding(TextBox.TextProperty, bindingText);

            // just to see something if we haven't updated /Strings/languages...
            if (string.IsNullOrWhiteSpace(tb.Header.ToString()))
                if (property.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo attrib)
                    tb.Header = attrib.SQLName;

            return tb;
        }
        #endregion

        #region ImageBox
        ImageBox CreateImageBox(PropertyInfo property, string baseModelTypeName, BaseModel selectedItem)
        {
            UniImage uniImage = property.GetValue(selectedItem) as UniImage ?? new UniImage();

            string memberName = ResourceLoader.GetString($"textbox_{baseModelTypeName}_{property.Name}");
            if (string.IsNullOrWhiteSpace(memberName))
                memberName = property.Name;

            return new ImageBox { DataContext = new ImageBoxVM(uniImage, property, selectedItem, memberName) };
        }
        #endregion

        #region GridBox
        GridBox CreateGridBox(PropertyInfo property, string baseModelTypeName, BaseModel selectedItem, Type vmType)
        {
            string name = ResourceLoader.GetString($"textbox_{baseModelTypeName}_{property.Name}");
            if (string.IsNullOrWhiteSpace(name))
                name = property.Name;

            Type newItemType = ViewModelResolver.GetNewItemVMType(EnControlTypes.GridBox);
            Type editItemType = ViewModelResolver.GetEditItemVMType(EnControlTypes.GridBox);
            var vmArgs = new object[] { selectedItem, property.GetValue(selectedItem), name, property, newItemType, editItemType };

            var gridBox = new GridBox()
            {
                DataContext = BuildVM(property.PropertyType.GetGenericArguments()[0], vmType, vmArgs)
            };

            //TODO mettere in vm gridbox (da cui derivano tutti gli altri) un const cui le view fanno binding)
            gridBox.MinWidth = 400;

            return gridBox;
        }

        #endregion

        #region GridBoxView
        GridBox CreateGridBoxView(PropertyInfo property, string baseModelTypeName, BaseModel selectedItem, Type vmType)
        {
            string name = ResourceLoader.GetString($"textbox_{baseModelTypeName}_{property.Name}");
            if (string.IsNullOrWhiteSpace(name))
                name = property.Name;

            var vmArgs = new object[] { property.GetValue(selectedItem), name };

            var gridBox = new GridBox()
            {
                DataContext = BuildVM(property.PropertyType.GetGenericArguments()[0], vmType, vmArgs)
            };
            gridBox.MinWidth = 400;

            return gridBox;
        }
        #endregion

        #region GridBoxDataSet
        GridBox CreateGridBoxDataSet(PropertyInfo property, string baseModelTypeName, BaseModel selectedItem, Type vmType)
        {
            string name = ResourceLoader.GetString($"textbox_{baseModelTypeName}_{property.Name}");
            if (string.IsNullOrWhiteSpace(name))
                name = property.Name;

            Type newItemType = ViewModelResolver.GetNewItemVMType(EnControlTypes.GridBoxDataSet);
            Type editItemType = ViewModelResolver.GetEditItemVMType(EnControlTypes.GridBoxDataSet);
            var vmArgs = new object[] { selectedItem, property.GetValue(selectedItem), name, property, newItemType, editItemType };

            var gridBox = new GridBox()
            {
                DataContext = BuildVM(property.PropertyType.GetGenericArguments()[0], vmType, vmArgs)
            };
            gridBox.MinWidth = 400;

            return gridBox;
        }
        #endregion

        #region GridBoxMtM
        GridBox CreateGridBoxMtM(PropertyInfo property, string baseModelTypeName, BaseModel selectedItem, Type vmType)
        {
            string dependencyFilterPropertyName = string.Empty;
            string parentFilterPropertyName = string.Empty;
            string dependencyFilterPropertyValue = string.Empty;

            if (property.GetCustomAttribute(typeof(RenderInfo)) is RenderInfo renderInfo)
            {
                dependencyFilterPropertyName = renderInfo.DependencyFilterPropertyName;
                parentFilterPropertyName = renderInfo.ParentFilterPropertyName;
                dependencyFilterPropertyValue = renderInfo.DependencyFilterPropertyValue;
            }

            string name = ResourceLoader.GetString($"textbox_{baseModelTypeName}_{property.Name}");
            if (string.IsNullOrWhiteSpace(name))
                name = property.Name;

            Type newItemType = ViewModelResolver.GetNewItemVMType(EnControlTypes.GridBoxMtM);
            Type editItemType = ViewModelResolver.GetEditItemVMType(EnControlTypes.GridBoxMtM);
            Type showboxType = ViewModelResolver.GetShowBoxVMType(EnControlTypes.GridBoxMtM);
            var vmArgs = new object[] { selectedItem, property.GetValue(selectedItem), name, property, dependencyFilterPropertyName, parentFilterPropertyName, dependencyFilterPropertyValue, newItemType, editItemType, showboxType };

            var gridBox = new GridBox()
            {
                DataContext = BuildVM(property.PropertyType.GetGenericArguments()[0], vmType, vmArgs)
            };
            gridBox.MinWidth = 400;
            return gridBox;
        }
        #endregion

        #region ToggleSwitch
        ToggleSwitch CreateToggleSwitch(PropertyInfo property, string baseModelTypeName)
        {
            var ts = new ToggleSwitch
            {
                OnContent = ResourceLoader.GetString("Yes"),
                OffContent = ResourceLoader.GetString("No"),
                Header = ResourceLoader.GetString($"textbox_{baseModelTypeName}_{property.Name}")
            };
            Binding bindingState = new Binding
            {
                Path = new PropertyPath($"SelectedItem.{property.Name}"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };

            ts.SetBinding(ToggleSwitch.IsOnProperty, bindingState);

            // just to see something if we haven't updated /Strings/languages...
            if (string.IsNullOrWhiteSpace(ts.Header.ToString()))
                if (property.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo attrib)
                    ts.Header = attrib.SQLName;

            return ts;
        }
        #endregion

        #region UniTextBox
        UniTextBox CreateUniTextBox(PropertyInfo property, string baseModelTypeName)
        {
            return new UniTextBox(property, baseModelTypeName);
        }
        #endregion

        #region DocumentBox
        DocumentBox CreateDocumentBox(PropertyInfo property, string baseModelTypeName, BaseModel selectedItem)
        {
            string memberName = ResourceLoader.GetString($"textbox_{baseModelTypeName}_{property.Name}");
            if (string.IsNullOrWhiteSpace(memberName))
                memberName = property.Name;

            var documentBoxVM = new DocumentBoxVM(memberName, property, selectedItem);

            var documentBox = new DocumentBox() { DataContext = documentBoxVM };

            return documentBox;

        }
        #endregion

        #region ShowBox
        ShowBox CreateShowBox(PropertyInfo property, string baseModelTypeName, BaseModel selectedItem, Type vmType)
        {
            var filters = new ShowBoxFilters();
            if (property.GetCustomAttribute(typeof(RenderInfo)) is RenderInfo renderInfo)
            {
                filters.DependencyFilterPropertyName = renderInfo.DependencyFilterPropertyName;
                filters.ParentFilterPropertyName = renderInfo.ParentFilterPropertyName;
                filters.DependencyFilterPropertyValue = renderInfo.DependencyFilterPropertyValue;
            }

            string memberName = ResourceLoader.GetString($"textbox_{baseModelTypeName}_{property.Name}");
            if (string.IsNullOrWhiteSpace(memberName))
                memberName = property.Name;

            var newItemType = ViewModelResolver.GetNewItemVMType(EnControlTypes.ShowBox);
            var vmArgs = new object[] { property.GetValue(selectedItem), memberName, property, selectedItem, filters, newItemType };
            var showBoxVM = BuildVM(property.PropertyType, vmType, vmArgs);

            return new ShowBox() { DataContext = showBoxVM };
        }

        #endregion

        /// <summary>
        /// Custom controls use this to obtain their default or customized VM passed through ViewBuilder
        /// </summary>
        /// <returns></returns>
        private object BuildVM(Type propertyGenericArgument, Type vmType, object[] vmArgs = null)
        {
            // does a ctor with the specified number of parameters exist in the reflected class?
            var exceptionMessage = new StringBuilder();
            int argsCount = vmArgs.Length;
            ConstructorInfo ctor = vmType.GetConstructors().FirstOrDefault(c => c.GetParameters().Count() == argsCount);
            if (ctor == null)
            {
                exceptionMessage.AppendLine($"Cannot find suitable ctor in {vmType.GetType().Name} with following parameters:");
                exceptionMessage.AppendLine();

                foreach (object param in vmArgs)
                {
                    exceptionMessage.Append(param.ToString());
                    exceptionMessage.Append(" of type ");
                    exceptionMessage.Append(param.GetType().Name);
                    exceptionMessage.AppendLine();
                }

                throw new Exception(exceptionMessage.ToString());
            }

            // do all parameters match the type of the constructor's parameters?
            //#if DEBUG
            //            var mismatchingArgs = new List<(Type Arg, Type Param)>();

            //            if (ctor != null && vmArgs != null)
            //                for (int i = 0; i < argsCount; i++)
            //                    if (vmArgs[i] != null && ctor.GetParameters()[i] != null)
            //                        if (vmArgs[i].GetType() != ctor.GetParameters()[i].GetType())
            //                            mismatchingArgs.Add((vmArgs[i].GetType(), ctor.GetParameters()[i].GetType()));

            //            if (mismatchingArgs.Any())
            //            {
            //                exceptionMessage.AppendLine("Some of the arguments passed do not match the constructor found.");
            //                exceptionMessage.AppendLine();
            //                for (int i = 0; i < mismatchingArgs.Count; i++)
            //                {
            //                    exceptionMessage.AppendLine($"Arg no.{i}:");
            //                    exceptionMessage.AppendLine($"Expected type: {mismatchingArgs[i].Param.Name} - Passed type: {mismatchingArgs[i].Arg.Name}");
            //                    exceptionMessage.AppendLine();
            //                }

            //                throw new Exception(exceptionMessage.ToString());
            //            }
            //#endif

            if (vmType.IsGenericType)
                return Activator.CreateInstance(vmType.MakeGenericType(new Type[] { propertyGenericArgument }), vmArgs);
            else
            {
                if (ctor.GetParameters().Count() != 0)
                    return ctor.Invoke(vmArgs);
                else
                    return Activator.CreateInstance(vmType, vmArgs);
            }
        }

    }
}
