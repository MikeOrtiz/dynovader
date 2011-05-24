using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Phone.Net.NetworkInformation;

namespace MyScience
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            String filename = value.ToString();
            BitmapImage image;
            if (NetworkInterface.GetIsNetworkAvailable()&& filename.StartsWith("http"))
            {
                image = new BitmapImage(new Uri(value.ToString()));
                return image;
            } else if (!NetworkInterface.GetIsNetworkAvailable() && filename.StartsWith("http")){
                filename = filename.Substring(filename.LastIndexOf('/') + 1);
            }
            //else
            //{
            //    String filename = value.ToString() + ".jpg";
            if (!filename.EndsWith(".jpg")) filename += ".jpg";
                image = new BitmapImage();
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!myIsolatedStorage.FileExists("MyScience/Images/" + filename)) return null;
                    using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile("MyScience/Images/" + filename, FileMode.Open, FileAccess.Read))
                    {
                        image.SetSource(fileStream);
                    }
                }
                return image;
            //}

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
