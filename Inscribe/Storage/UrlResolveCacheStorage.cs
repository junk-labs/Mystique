using System.Collections.Concurrent;

namespace Inscribe.Storage
{
    public static class UrlResolveCacheStorage
    {
        private static ConcurrentDictionary<string, string> urlResolved = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// URLがキャッシュされている場合、キャッシュ結果を返します。<para />
        /// キャッシュが無い場合はnullを返します。
        /// </summary>
        public static string Lookup(string url)
        {
            if (urlResolved.ContainsKey(url))
            {
                return urlResolved[url];
            }
            else
            {
                return null;
            }
        }

        public static void AddResolved(string original, string resolved)
        {
            urlResolved.AddOrUpdate(original, resolved);
            urlResolved.AddOrUpdate(resolved, resolved);
        }
    }
}
