using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inscribe.Threading;
using System.Windows;
using System.Windows.Controls;
using Octave.Caching;
using System.Windows.Threading;

namespace Mystique.Views.Common
{
    public class LazyImage : Image
    {
        static QueueTaskDispatcher taskrun;

        static LazyImage()
        {
            taskrun = new QueueTaskDispatcher(2);
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

        private static void UriSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var img = o as LazyImage;
            var uri = e.NewValue as Uri;
            if (img == null) return;
            taskrun.Enqueue(() =>
            {
                if (uri != null)
                {
                    var bi = ImageCacheStorage.DownloadImage(uri);
                    if (bi != null)
                    {
                        img.Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                if (img.UriSource == uri)
                                    img.Source = bi.Clone();
                            }
                            catch { }
                        }, DispatcherPriority.ContextIdle);

                    }
                }
                else
                {
                    img.Dispatcher.BeginInvoke(() =>
                    {
                        img.Source = null;
                    }, DispatcherPriority.ContextIdle);
                }
            });
        }
    }
}
