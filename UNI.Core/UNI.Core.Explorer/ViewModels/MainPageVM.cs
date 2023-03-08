using Gioiaspa.Warehouse.Library;
using System;
using System.Collections.Generic;
using UNI.Core.Explorer.ViewModels.ListGrid;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI;
using UNI.Core.UI.Menu;
using UNI.Core.UI.Tabs.ListDetail;
using UNI.Core.UI.Tabs.ListGrid;
using Windows.UI.Xaml.Controls;

namespace UNI.Core.Explorer.ViewModels
{
    public class MainPageVM : UI.MainPage.MainPageVM
    {
        public MainPageVM()
        {
            MainWindowTitle = "UNI Explorer Test";

            DisableDefaultVMBehaviour();
            MenuBuilder();
        }

        public override void DisableDefaultVMBehaviour()
        {
            var extendedVms = new List<Type>()
            {
                typeof(ListGridVMBordero),

            };

            UNICompositionRoot.BlockBaseViewModels(extendedVms);
        }

        public override void MenuBuilder()
        {
            MenuNodes = new List<MenuNode>
            {
                new MenuNode()
                {
                    Name = "Magazzino e Servizi",
                    Children = new List<MenuNode>
                    {
                        new MenuNode() { Name = "Commesse", Icon = new FontIcon() { Glyph = "\uE8F1" }, ViewModelType = typeof(ListGridVMCommesse) },
                        new MenuNode() { Name = "TEST Prodotti", Icon = new FontIcon() { Glyph = "\uE71D" },ViewModelType = typeof(ListGridProductVM) },
                        new MenuNode() { Name = "TEST Ordini di acquisto", Icon = new FontIcon() { Glyph = "\uE71D" },ViewModelType = typeof(ListGridVM<PurchaseOrder>) },
                        new MenuNode() { Name = "TEST Ordini di vendita", Icon = new FontIcon() { Glyph = "\uE71D" },ViewModelType = typeof(ListGridVM<SalesOrder>) },
                        new MenuNode() { Name = "TEST DDT", Icon = new FontIcon() { Glyph = "\uE71D" },ViewModelType = typeof(ListGridVM<DDT>) },
                        new MenuNode() { Name = "TEST Bordero", Icon = new FontIcon() { Glyph = "\uE71D" },ViewModelType = typeof(ListGridVMBordero) },
                        new MenuNode() { Name = "TEST Movimenti", Icon = new FontIcon() { Glyph = "\uE71D" },ViewModelType = typeof(ListGridVM<ProductMovement >) },
                    }
                },
                new MenuNode() { Name = "TEST ListDetail", Icon = new FontIcon() { Glyph = "\uE716" }, ViewModelType = typeof(ListDetailVM<BaseProduct>) },
                new MenuNode() { Name = "TEST Employee", Icon = new FontIcon() { Glyph = "\uE71D" }, ViewModelType = typeof(ListGridVM<Employee>) }

            };
        }
    }
}