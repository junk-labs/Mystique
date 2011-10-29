using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Dulcet.Network;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.Data;

namespace Inscribe.Storage
{
    public static class ImageCacheStorage
    {
        public const int ImageMaxWidth = 48;


        private static ReaderWriterLockWrap lockWrap;
        private static Dictionary<Uri, KeyValuePair<BitmapImage, DateTime>> imageDataDictionary;
        private static object semaphoreAccessLocker = new object();
        private static Dictionary<Uri, ManualResetEvent> semaphores;

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
            lockWrap = new ReaderWriterLockWrap();
            imageDataDictionary = new Dictionary<Uri, KeyValuePair<BitmapImage, DateTime>>();
            semaphores = new Dictionary<Uri, ManualResetEvent>();
            gcTimer = new Timer(GC, null, Setting.Instance.KernelProperty.ImageGCInitialDelay, Setting.Instance.KernelProperty.ImageGCInterval);
        }

        private static void GC(object o)
        {
            using (lockWrap.GetWriterLock())
            {
                imageDataDictionary
                    .Where(d => DateTime.Now.Subtract(d.Value.Value).TotalMilliseconds > Setting.Instance.KernelProperty.ImageLifetime)
                    .Concat(imageDataDictionary.OrderByDescending(v => v.Value.Value)
                    .Skip((int)(Setting.Instance.KernelProperty.ImageCacheMaxCount * Setting.Instance.KernelProperty.ImageCacheSurviveDensity)))
                    .Select(d => d.Key)
                    .ToArray()
                    .ForEach(d => imageDataDictionary.Remove(d));
            }
        }

        public static void ClearAllCache()
        {
            using (lockWrap.GetWriterLock())
            {
                imageDataDictionary.Clear();
            }
        }

        private static void RemoveCache(Uri key)
        {
            KeyValuePair<BitmapImage, DateTime> kv;
            using(lockWrap.GetWriterLock())
            {
                imageDataDictionary.Remove(key);
            }
        }

        /// <summary>
        /// キャッシュがヒットした場合のみBitmapImageを返します。<para />
        /// ヒットしなかった場合はnullが返ります。
        /// </summary>
        public static BitmapImage GetImageCache(Uri uri)
        {
            KeyValuePair<BitmapImage, DateTime> cdata;
            bool hot = false;
            using (lockWrap.GetReaderLock())
            {
                hot = imageDataDictionary.TryGetValue(uri, out cdata);
            }
            if (hot && DateTime.Now.Subtract(cdata.Value).TotalMilliseconds <= Setting.Instance.KernelProperty.ImageLifetime)
                return cdata.Key;
            // return cdata.Key.CloneFreeze();
            else
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
                    semaphores.Add(uri, new ManualResetEvent(false));
                }
            }
            if (mre != null)
            {
                mre.WaitOne();
                return GetImageCache(uri); // return cached image
            }
            try
            {
                var condata =
                    Http.WebConnect(
                    Http.CreateRequest(uri, contentType: null),
                    Http.StreamConverters.ReadStream);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
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
                // Save the memory.
                bi.DecodePixelWidth = ImageMaxWidth;
                bi.EndInit();
                bi.Freeze();

                var newv = new KeyValuePair<BitmapImage, DateTime>(bi, DateTime.Now);
                using (lockWrap.GetWriterLock())
                {
                    if (imageDataDictionary.ContainsKey(uri))
                    {
                        imageDataDictionary[uri] = newv;
                    }
                    else
                    {
                        imageDataDictionary.Add(uri, newv);
                        if (imageDataDictionary.Count() > Setting.Instance.KernelProperty.ImageCacheMaxCount)
                            Task.Factory.StartNew(() => GC(null));
                    }
                }
                return bi;
                // return bi.CloneFreeze();
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
