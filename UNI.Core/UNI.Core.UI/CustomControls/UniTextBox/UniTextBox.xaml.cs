using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UNI.Core.Library;
using UNI.Core.Library.Mapping;
using UNI.Core.Library.ValidationRules;
using UNI.Core.UI.Misc;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

// Il modello di elemento Controllo utente è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=234236

namespace UNI.Core.UI.CustomControls.UniTextBox
{
    public sealed partial class UniTextBox : UserControl
    {
        public TextBox TextBox { get => MainTextbox; }
        public List<IValidationRule> ValidationRules = new List<IValidationRule>();
        public bool ValidationSuccesful { get; set; } = true;
        public PropertyInfo PropertyInfo { get; set; }

        public UniTextBox(PropertyInfo property, string baseModelTypeName)
        {
            this.InitializeComponent();

            PropertyInfo = property;

            TextBox.Header = ResourceLoader.GetForCurrentView().GetString($"textbox_{baseModelTypeName}_{property.Name}");
            ValidationRules = (property.GetCustomAttributes(typeof(MandatoryFieldValidationRule)) as IEnumerable<IValidationRule>).ToList();

            var bindingText = new Binding { Path = new PropertyPath($"SelectedItem.{property.Name}") };

            if (property.DeclaringType.Name == "BaseModel")
            {
                bindingText.Mode = BindingMode.OneWay;
                bindingText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                TextBox.IsReadOnly = true;
            }
            else
            {
                bindingText.Mode = BindingMode.TwoWay;
                bindingText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            }

            if (property.GetCustomAttribute(typeof(RenderInfo)) is RenderInfo renderInfo)
                if (renderInfo.Converter != null)
                    bindingText.Converter = Activator.CreateInstance(PlatformTypeConverterHelper.GetSpecificValueConverter(renderInfo.Converter)) as IValueConverter;
            TextBox.SetBinding(TextBox.TextProperty, bindingText);

            if (property.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo attrib)
            {
                // just to see something if we haven't updated /Strings/languages...
                if (string.IsNullOrWhiteSpace(TextBox.Header.ToString()))
                    TextBox.Header = attrib.SQLName;

                //set the readonly status based on the valueinfo
                TextBox.IsReadOnly = attrib.IsReadOnly;
                if (!attrib.IsReadOnly)
                    TextBox.IsReadOnly = attrib.IsUserReadOnly;
            }
        }

        private void ValidateEntry()
        {
            string allErrorMessages = string.Empty;
            ValidationSuccesful = true;
            foreach (IValidationRule validationRule in ValidationRules ?? new List<IValidationRule>())
            {
                string errormessage = string.Empty;
                if (!validationRule.Validate(MainTextbox.Text, out errormessage))
                {
                    MainTextbox.BorderBrush = new SolidColorBrush(Colors.Red);
                    ValidationSuccesful = false;
                }
                else
                    MainTextbox.BorderBrush = new SolidColorBrush((Color)this.Resources["SystemBaseHighColor"]);
                allErrorMessages += errormessage;
            }
            ValidationErrorTextBlock.Text = allErrorMessages;
        }

        private void MainTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateEntry();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ValidateEntry();
        }
    }
}
