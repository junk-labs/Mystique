using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Inscribe.Common;
using Inscribe.Storage;

namespace Mystique.Views.Common
{
    public class LazyImage : Image
    {

        static QueueTaskDispatcher taskrun;

        static LazyImage()
        {
            taskrun = new QueueTaskDispatcher(1);
            ThreadHelper.Halt += taskrun.Dispose;
        }

        public Uri UriSource
        {
            get { return (Uri)GetValue(UriSourceProperty); }
            set { SetValue(UriSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UriSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UriSourceProperty =
            DependencyProperty.Register("UriSource", typeof(Uri), typeof(LazyImage), new UIPropertyMetadata(null, UriSourcePropertyChanged));

        public ImageSource DefaultImage
        {
            get { return (ImageSource)GetValue(DefaultImageProperty); }
            set { SetValue(DefaultImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultImageProperty =
            DependencyProperty.Register("DefaultImage", typeof(ImageSource), typeof(LazyImage), new UIPropertyMetadata(null));

        private static void UriSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var img = o as LazyImage;
            var uri = e.NewValue as Uri;
            if (img == null) return;
            if (uri != null)
            {
                try
                {
                    if (uri.Scheme == "pack")
                    {
                        var bi = new BitmapImage(uri).AsFreeze();
                        SetImage(img, bi, uri);
                    }
                    else
                    {
                        bool hitCache = false;
                        try
                        {
                            var cache = ImageCacheStorage.GetImageCache(uri);
                            if (cache != null)
                            {
                                SetImage(img, cache, uri);
                                hitCache = true;
                            }
                            else
                            {
                                if (img.DefaultImage == null || img.DefaultImage == DependencyProperty.UnsetValue)
                                {
                                    // img.Source = null causes binding error.
                                    img.SetValue(Image.SourceProperty, DependencyProperty.UnsetValue);
                                }
                                else
                                {
                                    img.Source = img.DefaultImage.CloneFreezeNew();
                                }
                            }
                        }
                        catch
                        {
                            hitCache = false;
                        }
                        if (!hitCache)
                        {
                            taskrun.Enqueue(() =>
                            {
                                try
                                {
                                    var bi = ImageCacheStorage.DownloadImage(uri);
                                    Application.Current.Dispatcher.BeginInvoke(() => SetImage(img, bi, uri),
                                        DispatcherPriority.ContextIdle);
                                }
                                catch { }
                            });
                        }
                    }
                }
                catch { }
            }
        }

        private static void SetImage(LazyImage image, ImageSource bitmap, Uri checkUri)
        {
            try
            {
                if (bitmap != null && (checkUri == null || image.UriSource == checkUri))
                {
                    image.Source = bitmap;
                }
            }
            catch { }
        }
    }
}
