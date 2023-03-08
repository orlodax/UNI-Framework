using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using UNI.API.Contracts.RequestsDTO;
using UNI.Core.Client;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.UI.MainPage;
using UNI.Core.UI.Misc;
using UNI.Core.UI.Tabs;
using Windows.Storage.Pickers;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace UNI.Core.UI.CustomControls
{
    public class DocumentBoxVM : BaseTabVMTypeAgnostic
    {
        private Document document;
        private string memberName;
        private string name;

        private readonly PropertyInfo propertyInfo;
        private readonly BaseModel parent;

        private readonly UniClient<Document> baseClient;

        public DocumentBoxVM(string memberName, PropertyInfo propertyInfo, BaseModel parent)
        {
            baseClient = new UniClient<Document>();

            this.memberName = memberName;
            this.propertyInfo = propertyInfo;
            this.parent = parent;

            OpenDocumentViewer = new RelayCommand(OpenDocumentViewerCommand);
            AddDocument = new RelayCommand(AddDocumentCommand);
            DeleteDocument = new RelayCommand(DeleteDocumentCommand);

            LoadData();
        }

        private async void LoadData()
        {
            string parentTable = string.Empty;
            if (parent.GetType().GetCustomAttribute(typeof(ClassInfo)) is ClassInfo classInfo)
            {
                parentTable = classInfo.SQLName;
            }


            FilterExpression filterExpressionTable = new FilterExpression() { PropertyName = "ReferTable", PropertyValue = parentTable };
            FilterExpression filterExpressionId = new FilterExpression() { PropertyName = "ReferId", PropertyValue = parent.ID.ToString() };

            List<FilterExpression> filterExpressions = new List<FilterExpression>
            {
                filterExpressionTable,
                filterExpressionId
            };

            List<Document> documents = await baseClient.Get(new GetDataSetRequestDTO() { FilterExpressions = filterExpressions }) ?? new List<Document>();
            if (documents.Any())
                document = documents[0];
            else document = null;

            Name = document?.Name ?? string.Empty;

            //Init the document if it is null
            if (document == null)
            {
                document = new Document
                {
                    Name = propertyInfo.Name,
                    ReferTable = parentTable,
                    ReferId = parent.ID
                };
            }
        }

        private async void CreateOrUpdateDocument()
        {
            document.File = await PickFile();
            document.FileName = memberName + ".pdf";
            if (document.ID == 0)
            {
                await baseClient.CreateItem(document);
            }
            else
            {
                await baseClient.UpdateItem(document);
            }
            LoadData();

        }


        public Document Document { get => document; set { document = value; NotifyPropertyChanged(); } }
        public ICommand OpenDocumentViewer { get; set; }
        public ICommand AddDocument { get; set; }
        public ICommand DeleteDocument { get; set; }
        public string MemberName { get => memberName; set { memberName = value; NotifyPropertyChanged(); } }
        public string Name { get => name; set { name = value; NotifyPropertyChanged(); } }


        public async void OpenDocumentViewerCommand(object parameter)
        {
            if (document != null)
            {

                if (document.File != null)
                {
                    AppWindow appWindow = await AppWindow.TryCreateAsync();
                    appWindow.Title = document.Name;
                    //// Create a Frame and navigate to the Page you want to show in the new window.
                    Frame appWindowContentFrame = new Frame();
                    appWindowContentFrame.Navigate(typeof(PdfViewer.PdfViewer), document);

                    // Attach the XAML content to the window.
                    ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);

                    await appWindow.TryShowAsync();
                    //// Get a reference to the page instance and assign the
                    //// newly created AppWindow to the MyAppWindow property.
                    //AppWindowPage page = (AppWindowPage)appWindowContentFrame.Content;
                    //page.MyAppWindow = appWindow;
                }
            }
        }

        public void AddDocumentCommand(object parameter)
        {
            if (document?.File != null)
            {
                ContentDialog cd = new ContentDialog()
                {
                    Title = "Attenzione",
                    Content = "E' stato già rilevato un documento caricato, sei sicuro di volerlo sostituire? Non sarà possibile recuperare il precedente documento",
                    CloseButtonText = "Annulla",
                    PrimaryButtonText = "Continua",
                };

                cd.PrimaryButtonClick += Cd_PrimaryButtonClick;

                TabViewVM.ShowContentDialog(cd);
            }
            else CreateOrUpdateDocument();
        }

        private void Cd_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            CreateOrUpdateDocument();
        }

        public void DeleteDocumentCommand(object parameter)
        {
            ContentDialog cd = new ContentDialog()
            {
                Title = "Attenzione",
                Content = "Sei sicuro di voler eliminare il file? Non sarà possibile recuperare il documento eliminato",
                CloseButtonText = "Annulla",
                PrimaryButtonText = "Continua",
            };

            cd.PrimaryButtonClick += Cd_PrimaryButtonDeleteClick;

            TabViewVM.ShowContentDialog(cd);
        }
        private async void Cd_PrimaryButtonDeleteClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (document.ID != 0)
            {
                await baseClient.DeleteItem(document);
                LoadData();
            }
        }

        private async Task<byte[]> PickFile()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".pdf");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var stream = await file.OpenStreamForReadAsync();
                var bytes = new byte[(int)stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                return bytes;
            }
            else
            {
                return null;
            }
        }

    }
}
