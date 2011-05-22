using Inscribe.Data;

namespace Octave.Caching
{
    public static class UrlResolveCacheStorage
    {
        private static SafeDictionary<string, string> urlResolved = new SafeDictionary<string, string>();

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

        private static SafeDictionary<string, string> imageResolved = new SafeDictionary<string, string>();

        /// <summary>
        /// URLがキャッシュされている場合、キャッシュ結果を返します。<para />
        /// キャッシュが無い場合はnullを返します。
        /// </summary>
        public static string LookupImage(string url)
        {
            if (imageResolved.ContainsKey(url))
            {
                return imageResolved[url];
            }
            else
            {
                return null;
            }
        }

        public static void AddResolvedImage(string original, string resolved)
        {
            imageResolved.AddOrUpdate(original, resolved);
        }
    }
}
