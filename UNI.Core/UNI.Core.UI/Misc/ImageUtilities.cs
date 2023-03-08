using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace UNI.Core.UI.Misc
{
    public class ImageUtilities
    {
        //CONVERSION UTILITIES---------------------------------------------------------------------------------
        //public object Convert(object value, Type targetType, object parameter, string language)
        //{
        //    var imageBytes = (byte[])value;
        //    return ConvertToImage(imageBytes);
        //}

        //public object ConvertBack(object value, Type targetType, object parameter, string language)
        //{
        //    throw new NotImplementedException();
        //}

        public static async Task<BitmapImage> ConvertToImage(byte[] imageBytes)
        {
            var image = new BitmapImage();
            if (imageBytes != null)
            {
                if (imageBytes.Count() > 0)
                {
                    using (var randomAccessStream = new InMemoryRandomAccessStream())
                    {
                        var dw = new DataWriter(randomAccessStream.GetOutputStreamAt(0));
                        dw.WriteBytes(imageBytes);
                        await dw.StoreAsync();
                        await image.SetSourceAsync(randomAccessStream);
                    }
                    return image;
                }
            }
            return null;
        }

        public static async Task<WriteableBitmap> ConvertToWriteable(byte[] imageBytes, BitmapImage bmig)
        {
            var image = new WriteableBitmap(bmig.PixelWidth, bmig.PixelHeight);
            if (imageBytes != null)
            {
                using (var randomAccessStream = new InMemoryRandomAccessStream())
                {
                    var dw = new DataWriter(randomAccessStream.GetOutputStreamAt(0));
                    dw.WriteBytes(imageBytes);
                    await dw.StoreAsync();
                    await image.SetSourceAsync(randomAccessStream);
                }
            }
            return image;
        }

        public static async Task<Byte[]> FileToByteArray(StorageFile file)
        {
            //var file = await StorageFile.GetFileFromApplicationUriAsync(sourceUri);
            using (var inputStream = await file.OpenSequentialReadAsync())
            {
                var readStream = inputStream.AsStreamForRead();
                var buffer = new byte[readStream.Length];
                await readStream.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }
        }
        //-------------------------------------------------------------------------------------------------------

        //converts file selected in storage file as bitmapimage to feed imagesource
        public static async Task<BitmapImage> PreviewImage(StorageFile file)
        {
            BitmapImage bitmapImage = new BitmapImage();
            FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);

            bitmapImage.SetSource(stream);

            return bitmapImage;
        }

        //RESIZE UPLOADED PICTURES 
        public static async Task<BitmapImage> ResizeImage(StorageFile ImageFile, int maxWidth, int maxHeight)
        {
            IRandomAccessStream inputstream = await ImageFile.OpenReadAsync();
            BitmapImage sourceImage = new BitmapImage();
            sourceImage.SetSource(inputstream);
            var origHeight = sourceImage.PixelHeight;
            var origWidth = sourceImage.PixelWidth;
            var ratioX = maxWidth / (float)origWidth;
            var ratioY = maxHeight / (float)origHeight;
            var ratio = Math.Min(ratioX, ratioY);
            var newHeight = (int)(origHeight * ratio);
            var newWidth = (int)(origWidth * ratio);

            sourceImage.DecodePixelWidth = newWidth;
            sourceImage.DecodePixelHeight = newHeight;

            return sourceImage;
        }
    }
}
