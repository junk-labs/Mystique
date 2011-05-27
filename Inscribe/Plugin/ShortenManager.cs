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
        private static List<IURLShortener> shorteners = new List<IURLShortener>();

        private static List<IURLExtractor> extractors = new List<IURLExtractor>();

        public static IEnumerable<IURLShortener> Shorteners
        {
            get { return shorteners; }
        }

        public static IEnumerable<IURLExtractor> Extractors
        {
            get { return extractors; }
        }

        public static void RegisterShortener(IURLShortener shortener)
        {
            shorteners.Add(shortener);
        }

        public static void RegisterExtractor(IURLExtractor extractor)
        {
            extractors.Add(extractor);
        }

        public static void RegisterAllShortenersInAsm(Assembly asm)
        {
            foreach (var type in asm.GetTypes())
            {
                var ifs = type.GetInterfaces();
                if (ifs.Contains(typeof(IURLShortener)))
                {
                    RegisterShortener(Activator.CreateInstance(type) as IURLShortener);
                }
                else if (ifs.Contains(typeof(IURLExtractor)))
                {
                    RegisterExtractor(Activator.CreateInstance(type) as IURLExtractor);
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
        public static IURLShortener GetSuggestedShortener()
        {
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
        }
    }
}
