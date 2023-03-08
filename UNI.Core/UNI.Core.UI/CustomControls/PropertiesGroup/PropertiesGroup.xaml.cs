using System.Collections.Generic;
using UNI.Core.Library;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Il modello di elemento Controllo utente è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=234236

// TODO refactor con MVVM (forse)

namespace UNI.Core.UI.CustomControls.PropertiesGroup
{
    public sealed partial class PropertiesGroup : UserControl
    {
        public new string Name { get; set; }
        public List<FrameworkElement> Controls { get; set; } = new List<FrameworkElement>();
        public string DisplayName { get; set; }

        public PropertiesGroup(string groupName, string displayName)
        {
            this.InitializeComponent();
            Name = groupName;
            DisplayName = displayName;
            GroupName.Text = displayName;

            if (string.IsNullOrEmpty(displayName))
                GroupName.Text = $"displayName is missing, using groupName: {groupName}";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vb = new PropertiesGroupVB<BaseModel>();
            if (DetailsContainer.Content == null)
                DetailsContainer.Content = vb.RenderGrid(Controls);
        }
    }
}
