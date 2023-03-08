using System;
using System.Collections.ObjectModel;
using System.IO;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using Windows.Data.Pdf;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace UNI.Core.UI.CustomControls.PdfViewer
{
    public class PdfViewerVM : BaseModel
    {

        private ObservableCollection<BitmapImage> pdfPages = new ObservableCollection<BitmapImage>();
        private bool isProgressRingActive = true;
        private readonly Document document;

        public ObservableCollection<BitmapImage> PdfPages { get { return pdfPages; } set { pdfPages = value; NotifyPropertyChanged(); } }
        public bool IsProgressRingActive { get => isProgressRingActive; set { isProgressRingActive = value; NotifyPropertyChanged(); } }



        public PdfViewerVM(Document document)
        {
            this.document = document;
            LoadDocument();
        }

        private void LoadDocument()
        {
            IsProgressRingActive = true;
            if (document != null)
            {
                LoadDocumentView(document);
            }
            else
            {
                PdfPages = new ObservableCollection<BitmapImage>();
            }
            IsProgressRingActive = false;

        }


        async void LoadDocumentView(Document doc)
        {
            try
            {
                Stream streamDoc = new MemoryStream(doc.File);
                PdfDocument pdfDoc = await PdfDocument.LoadFromStreamAsync(streamDoc.AsRandomAccessStream());
                PdfPages.Clear();

                for (uint i = 0; i < pdfDoc.PageCount; i++)
                {
                    BitmapImage image = new BitmapImage();

                    var page = pdfDoc.GetPage(i);

                    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                    {
                        await page.RenderToStreamAsync(stream);
                        await image.SetSourceAsync(stream);
                    }

                    PdfPages.Add(image);
                }
            }
            catch (Exception)
            {

            }

        }
    }
}
