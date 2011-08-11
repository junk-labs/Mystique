using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using Dulcet.Network;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.Storage;

namespace Inscribe.Storage
{
    public static class ImageCacheStorage
    {
        private static ConcurrentDictionary<Uri, KeyValuePair<BitmapImage, DateTime>> imageDataDictionary;
        private static object semaphoreAccessLocker = new object();
        private static ConcurrentDictionary<Uri, ManualResetEvent> semaphores;

        public static event Action DownloadingChanged = () => { };
        private static bool _downloading = false;
        public static bool Downloading
        {
            get { return _downloading; }
            private set
            {
                _downloading = value;
                DownloadingChanged();
            }
        }

        private static Timer gcTimer;

        static ImageCacheStorage()
        {
            imageDataDictionary = new ConcurrentDictionary<Uri, KeyValuePair<BitmapImage, DateTime>>();
            semaphores = new ConcurrentDictionary<Uri, ManualResetEvent>();
            gcTimer = new Timer(GC, null, Setting.Instance.KernelProperty.ImageGCInitialDelay, Setting.Instance.KernelProperty.ImageGCInterval);
        }

        private static void GC(object o)
        {
            imageDataDictionary
                .Where(d => DateTime.Now.Subtract(d.Value.Value).TotalMilliseconds > Setting.Instance.KernelProperty.ImageLifetime)
                .Select(d => d.Key)
                .ToArray().AsParallel().ForAll(d => imageDataDictionary.Remove(d));
        }

        public static void ClearAllCache()
        {
            imageDataDictionary.Clear();
        }

        /// <summary>
        /// キャッシュがヒットした場合のみBitmapImageを返します。<para />
        /// ヒットしなかった場合はnullが返ります。
        /// </summary>
        public static BitmapImage GetImageCache(Uri uri)
        {
            KeyValuePair<BitmapImage, DateTime> cdata;
            if (imageDataDictionary.TryGetValue(uri, out cdata))
            {
                if (DateTime.Now.Subtract(cdata.Value).TotalMilliseconds <= Setting.Instance.KernelProperty.ImageLifetime)
                    return cdata.Key;
                System.Diagnostics.Debug.WriteLine("cache misshit:" + DateTime.Now.Subtract(cdata.Value).TotalMilliseconds.ToString());
            }
            return null;
        }

        /// <summary>
        /// 画像データを取得します。<para />
        /// 取得するまで呼び出し元はブロックされることに注意してください。
        /// </summary>
        public static BitmapImage DownloadImage(Uri uri)
        {
            var gid = GetImageCache(uri);
            if (gid != null)
                return gid;
            else
                return DownloadImageData(uri);
        }

        /// <summary>
        /// 画像データをダウンロードします。
        /// </summary>
        private static BitmapImage DownloadImageData(Uri uri)
        {
            ManualResetEvent mre;
            lock (semaphoreAccessLocker)
            {
                if (!semaphores.TryGetValue(uri, out mre))
                {
                    if (semaphores.Count == 0) Downloading = true;
                    semaphores.AddOrUpdate(uri, new ManualResetEvent(false));
                }
            }
            if (mre != null)
            {
                mre.WaitOne();
                return GetImageCache(uri); // return cached image
            }
            try
            {
                var bi = new BitmapImage();
                var condata =
                    Http.WebConnect(
                    Http.CreateRequest(uri, contentType: null),
                    Http.StreamConverters.ReadStream);
                bi.BeginInit();
                if (condata.Succeeded && condata.Data != null)
                {
                    bi.StreamSource = condata.Data;
                }
                else
                {
                    if (condata.ThrownException != null)
                        NotifyStorage.Notify("画像のロードエラー(" + uri.OriginalString + ")");
                    else
                        NotifyStorage.Notify(condata.ThrownException.Message);
                    bi.UriSource = "Resources/failed.png".ToPackUri();
                }
                bi.EndInit();
                bi.Freeze();
                imageDataDictionary.AddOrUpdate(uri, new KeyValuePair<BitmapImage, DateTime>(bi, DateTime.Now));
                return bi;
            }
            catch (Exception e)
            {
                NotifyStorage.Notify("画像のロードエラー(" + uri.OriginalString + "):" + e.ToString());
                return new BitmapImage("Resources/failed.png".ToPackUri());
            }
            finally
            {
                lock (semaphoreAccessLocker)
                {
                    if (semaphores.ContainsKey(uri))
                    {
                        semaphores[uri].Set();
                        semaphores.Remove(uri);
                        if (semaphores.Count == 0) Downloading = false;
                    }
                }
            }
        }
    }
}
