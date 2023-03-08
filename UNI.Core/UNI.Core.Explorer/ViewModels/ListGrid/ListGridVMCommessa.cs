using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UNI.Core.Library;
using UNI.Core.Library.Mapping;
using UNI.Core.UI.Components.SearchFilters;
using UNI.Core.UI.CustomControls.GridBox;
using UNI.Core.UI.Tabs.DetailItem;
using UNI.Core.UI.Tabs.ListGrid;
using Windows.UI.Xaml;

namespace UNI.Core.Explorer.ViewModels.ListGrid
{
    public class ListGridVMCommesse : ListGridVM<Commessa>
    {
        public ListGridVMCommesse()
        {
            SelectedItemsQuantity = 1000;
            DatePickerVisibility = Visibility.Visible;
            ExportButtonVisibility = Visibility.Visible;
            SetCustomProperties();
            _ = LoadData();
        }
        protected override async Task LoadData(List<FilterExpression> filterExpressions = null, object parameter = null)
        {
            await base.LoadData(filterExpressions, parameter);
            var orderedItemsSource = ItemsSource.OrderByDescending(i => i.Attiva);
            ItemsSource = new ObservableCollection<Commessa>(orderedItemsSource);
        }


        public void SetCustomProperties()
        {
            var visibleProps = new List<string>
            {
                 nameof(Commessa.Attiva),
                 nameof(Commessa.AziendaPrincipaleRagioneSociale),
                 nameof(Commessa.ClienteRagioneSociale),
            };
            ViewBuilder.CustomizeVisibleProperties(visibleProps);
            CreateItemVMType = typeof(DetailItemVMCommessa);
            DetailItemVMType = typeof(DetailItemVMCommessa);
            CustomizeSearchFilters(new SearchFilterSetup
            {
                PropertiesToShow = new List<string>
                {
                     nameof(Commessa.Attiva),
                     nameof(Commessa.AziendaPrincipale),
                     nameof(Commessa.Cliente),
                }
            });
        }
    }

    internal class DetailItemVMCommessa : DetailItemVM<Commessa>
    {
        public DetailItemVMCommessa(Commessa selectedItem) : base(selectedItem)
        {
            SetCustomProperties();
            DrawDetailsPane();
            Content = ViewBuilder.RenderMainFrameworkElement(selectedItem);
        }

        public void SetCustomProperties()
        {
            var visibleProps = new List<string>
            {
                 nameof(Commessa.Attiva),
                 nameof(Commessa.AziendaPrincipale),
                 nameof(Commessa.Cliente),
                 nameof(Commessa.AziendeSecondarie),
                 //nameof(Commessa.Servizi)
            };
            ViewBuilder.CustomizeVisibleProperties(visibleProps);
            ViewBuilder.CustomizeViewModelByPropertyName(nameof(Commessa.AziendeSecondarie), typeof(GridBoxVMAziende));
            //ViewBuilder.CustomizeViewModelByPropertyName(nameof(Commessa.Servizi), typeof(GridBoxVMServizi));
        }
    }

    internal class GridBoxVMAziende : GridBoxMtMVM<Azienda>
    {
        public GridBoxVMAziende(BaseModel parentItem,
                                         List<Azienda> itemsSource,
                                         string name,
                                         PropertyInfo propertyInfo,
                                         string dependencyFilterPropertyName,
                                         string parentFilterPropertyName,
                                         string depenencyFilterPropertyValue,
                                         Type newItemType,
                                         Type editItemType,
                                         Type showBoxVMType) : base(parentItem, itemsSource, name, propertyInfo, dependencyFilterPropertyName, parentFilterPropertyName, depenencyFilterPropertyValue, newItemType, editItemType, showBoxVMType)
        {
            SetCustomProperties();
            MainGrid = (DataGrid)ViewBuilder.RenderMainFrameworkElement(height: 300);
        }
        protected override void PopulateItemsSource(List<FilterExpression> filterExpressions = null)
        {
            filterExpressions = new List<FilterExpression>();
            base.PopulateItemsSource(filterExpressions);
        }
        public void SetCustomProperties()
        {
            var visibleProps = new List<string>
            {
                 nameof(Azienda.RagioneSociale),
            };
            ViewBuilder.CustomizeVisibleProperties(visibleProps);
        }
    }

    internal class GridBoxVMServizi : GridBoxMtMVM<Servizio>
    {
        public GridBoxVMServizi(BaseModel parentItem, List<Servizio> itemsSource, string name, PropertyInfo propertyInfo, string dependencyFilterPropertyName, string parentFilterPropertyName, string depenencyFilterPropertyValue, Type newItemType, Type editItemType, Type showBoxVMType) : base(parentItem, itemsSource, name, propertyInfo, dependencyFilterPropertyName, parentFilterPropertyName, depenencyFilterPropertyValue, newItemType, editItemType, showBoxVMType)
        {
            SetCustomProperties();
            MainGrid = (DataGrid)ViewBuilder.RenderMainFrameworkElement(height: 300);
        }
        protected override void PopulateItemsSource(List<FilterExpression> filterExpressions = null)
        {
            filterExpressions = new List<FilterExpression>();
            base.PopulateItemsSource(filterExpressions);
        }
        public void SetCustomProperties()
        {
            var visibleProps = new List<string>
            {
                 nameof(Servizio.Nome),
                 nameof(Servizio.Descrizione),
                 nameof(Servizio.UnitaDiMisura),
            };
            ViewBuilder.CustomizeVisibleProperties(visibleProps);
        }
    }

    [ClassInfo(SQLName = "commesse")]
    public class Commessa : BaseModel
    {
        /// <summary>
        /// Definisce se la commessa è attiva
        /// </summary>
        [ValueInfo(SQLName = "attiva")] public bool Attiva { get; set; }

        /// <summary>
        /// Cliente della commessa
        /// </summary>
        [ValueInfo()][RenderInfo(Group = "Aziende", NewLine = true)] public Azienda Cliente { get; set; }

        [ValueInfo(SQLName = "idcliente")] public int IdCliente { get; set; }

        /// <summary>
        /// Azienda della rete di impresa che ha in carico la commessa
        /// </summary>
        [ValueInfo()][RenderInfo(Group = "Aziende")] public Azienda AziendaPrincipale { get; set; }

        [ValueInfo(SQLName = "idaziendaprincipale", IsVisible = false)] public int IdAziendaPrincipale { get; set; }

        /// <summary>
        /// Altre aziende della rete di impresa che partecipano nella commessa
        /// </summary>
        [ValueInfo(ManyToManySQLName = "viewaziendecommesse", LinkTableSQLName = "commesseaziende")][RenderInfo(Group = "Aziende")] public List<Azienda> AziendeSecondarie { get; set; }

        /// <summary>
        ///  Servizi eseguiti nella commessa
        /// </summary>
        //[ValueInfo(ManyToManySQLName = "", LinkTableSQLName = "")][RenderInfo(Group = "Servizi", NewLine = true)] public List<Servizio> Servizi { get; set; }

        [ValueInfo(IsReadOnly = true)] public string ClienteRagioneSociale { get; set; }
        [ValueInfo(IsReadOnly = true, IsDisplayProperty = true)] public string AziendaPrincipaleRagioneSociale { get; set; }

        public Commessa()
        {
            if (Cliente != null)
            {
                ClienteRagioneSociale = Cliente.RagioneSociale;
            }
            if (AziendaPrincipale != null)
            {
                AziendaPrincipaleRagioneSociale = AziendaPrincipale.RagioneSociale;
            }
        }
    }

    [ClassInfo(SQLName = "aziende")]
    public class Azienda : BaseModel
    {
        [ValueInfo(SQLName = "ragionesociale", IsDisplayProperty = true)] public string RagioneSociale { get; set; }

    }

    [ClassInfo(SQLName = "servizi")]
    public class Servizio : BaseModel
    {
        //
        [ValueInfo(SQLName = "nome", IsDisplayProperty = true)] public string Nome { get; set; }

        //
        [ValueInfo(SQLName = "descrizione")] public string Descrizione { get; set; }

        //
        [ValueInfo(SQLName = "unitadimisura")] public string UnitaDiMisura { get; set; }
    }
}
