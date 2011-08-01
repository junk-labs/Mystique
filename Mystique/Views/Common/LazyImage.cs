using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Inscribe.Caching;
using Inscribe.Threading;

namespace Mystique.Views.Common
{
    public class LazyImage : Image
    {
        static QueueTaskDispatcher taskrun;

        static LazyImage()
        {
            taskrun = new QueueTaskDispatcher(4);
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
                        img.Dispatcher.BeginInvoke(() => SetImage(img, new BitmapImage(uri), uri));
                    }
                    else
                    {
                        var cache = ImageCacheStorage.GetImageCache(uri);
                        if (cache != null)
                        {
                            img.Dispatcher.BeginInvoke(() => SetImage(img, cache, uri));
                        }
                        else
                        {
                            img.Dispatcher.BeginInvoke(() =>
                            {
                                try
                                {
                                    img.Source = img.DefaultImage;
                                }
                                catch { }
                            });
                            taskrun.Enqueue(() =>
                            {
                                var bi = ImageCacheStorage.DownloadImage(uri);
                                img.Dispatcher.BeginInvoke(() => SetImage(img, bi, uri), DispatcherPriority.ContextIdle);
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
                if (bitmap != null)
                {
                    if (checkUri == null || image.UriSource == checkUri)
                        image.Source = bitmap;
                }
            }
            catch { }
        }
    }
}
