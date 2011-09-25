using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Acuerdo.External.Shortener;
using Inscribe.Configuration;

namespace Inscribe.Plugin
{
    public static class ShortenManager
    {
        private static List<IUrlShortener> shorteners = new List<IUrlShortener>();

        private static List<IUriExtractor> extractors = new List<IUriExtractor>();

        [Obsolete("Krile is no longer supported shortener services.")]
        public static IEnumerable<IUrlShortener> Shorteners
        {
            get { return shorteners; }
        }

        public static IEnumerable<IUriExtractor> Extractors
        {
            get { return extractors; }
        }

        [Obsolete("Krile is no longer supported shortener services.")]
        public static void RegisterShortener(IUrlShortener shortener)
        {
            shorteners.Add(shortener);
        }

        public static void RegisterExtractor(IUriExtractor extractor)
        {
            extractors.Add(extractor);
        }

        public static void RegisterAllShortenersInAsm(Assembly asm)
        {
            foreach (var type in asm.GetTypes())
            {
                var ifs = type.GetInterfaces();
                if (ifs.Contains(typeof(IUrlShortener)))
                {
                    RegisterShortener(Activator.CreateInstance(type) as IUrlShortener);
                }
                else if (ifs.Contains(typeof(IUriExtractor)))
                {
                    RegisterExtractor(Activator.CreateInstance(type) as IUriExtractor);
                }
            }
        }

        private static CommonExtractor commonExtractor = new CommonExtractor();

        /// <summary>
        /// URLの解決を試みます。<para />
        /// 何も無い場合は、そのままのURLを返します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Extract(string url)
        {
            string extd;
            foreach (var ext in extractors)
            {
                if (!ext.TryDecompress(url, out extd))
                {
                    return extd;
                }
            }
            if (commonExtractor.TryDecompress(url, out extd))
            {
                return extd;
            }
            return url;
        }

        /// <summary>
        /// URLがすでに短縮されているか確認します。
        /// </summary>
        /// <param name="url">確認するURL</param>
        /// <returns>短縮されていればtrue</returns>
        [Obsolete("Krile is no longer supported shortener services.")]
        public static bool IsShortedThis(string url)
        {
            foreach (var c in shorteners)
            {
                if (c.IsCompressed(url))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// URLの短縮を行うインスタンスを取得します。
        /// </summary>
        /// <returns>短縮サービスのインスタンス、指定がない場合はNULL</returns>
        [Obsolete("Krile is no longer supported shortener services.")]
        public static IUrlShortener GetSuggestedShortener()
        {
            throw new NotSupportedException();
            /*
            string sn = Setting.Instance.ExternalServiceProperty.ShortenerService;
            if (String.IsNullOrEmpty(sn)) return null;
            foreach (var ss in shorteners)
            {
                if (ss.Name == sn)
                {
                    return ss;
                }
            }
            return null;
            */
        }
    }
}
