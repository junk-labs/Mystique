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
using Inscribe.Util;

namespace Inscribe.Storage
{
    public static class ImageCacheStorage
    {
        private const double DefaultDpi = 96;

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
            gcTimer = new Timer(GCCacheStorage, null, Setting.Instance.KernelProperty.ImageCacheGCInitialDelay, Setting.Instance.KernelProperty.ImageCacheGCInterval);
        }

        private static bool _isGCing = false;
        private static void GCCacheStorage(object o)
        {
            if (_isGCing) return;
            _isGCing = true;

            System.Diagnostics.Debug.WriteLine("GC!");
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            IEnumerable<KeyValuePair<Uri, KeyValuePair<BitmapImage, DateTime>>> tuple = null;
            using (lockWrap.GetReaderLock())
            {
                tuple = imageDataDictionary.ToArray();
            }

            // 生き残るキャッシュの判定
            var surviver = tuple
                .Where(d => DateTime.Now.Subtract(d.Value.Value).TotalMilliseconds < Setting.Instance.KernelProperty.ImageLifetime)
                .OrderBy(v => v.Value.Value)
                .ToArray();
            var newdic = new Dictionary<Uri, KeyValuePair<BitmapImage, DateTime>>();
            surviver.ForEach(i => newdic.Add(i.Key, i.Value));

            using (lockWrap.GetWriterLock())
            {
                imageDataDictionary = newdic;
            }
            GC.Collect();

            sw.Stop();
            System.Diagnostics.Debug.WriteLine("GC completed: " + sw.ElapsedMilliseconds.ToString());

            _isGCing = false;
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
            if (hot)
            {
                if (DateTime.Now.Subtract(cdata.Value).TotalMilliseconds >= Setting.Instance.KernelProperty.ImageLifetime)
                {
                    // キャッシュ更新を非同期で行っておく
                    lock (semaphoreAccessLocker)
                    {
                        if (!semaphores.ContainsKey(uri)) // URLに対するDL予約があるなら何もしない
                            Task.Factory.StartNew(() => DownloadImageData(uri));
                    }
                }
                return cdata.Key;
            }
            else
            {
                return null;
            }
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
                    Http.CreateRequest(uri),
                    Http.StreamConverters.ReadStream);
                var bi = new BitmapImage();
                try
                {
                    if (condata.Succeeded && condata.Data != null)
                    {
                        using (var ws = new WrappingStream(condata.Data))
                        {
                            bi.BeginInit();
                            bi.CacheOption = BitmapCacheOption.OnLoad;
                            bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                            bi.DecodePixelWidth = ImageMaxWidth;
                            bi.StreamSource = ws;
                            bi.EndInit();
                            bi.Freeze();
                        }
                    }
                    else
                    {
                        if (condata.ThrownException != null)
                            NotifyStorage.Notify("画像のロードエラー(" + uri.OriginalString + ")");
                        else
                            NotifyStorage.Notify(condata.ThrownException.Message);
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.UriSource = "Resources/failed.png".ToPackUri();
                        bi.DecodePixelWidth = ImageMaxWidth;
                        bi.EndInit();
                        bi.Freeze();
                    }
                }
                finally
                {
                    if (condata.Data != null)
                        condata.Data.Dispose();
                }

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
                    }
                }
                return bi;
            }
            catch (Exception e)
            {
                NotifyStorage.Notify("画像のロードエラー(" + uri.OriginalString + "):" + e.ToString());
                return new BitmapImage("Resources/failed.png".ToPackUri()).AsFreeze();
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
