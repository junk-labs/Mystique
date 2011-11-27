using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Acuerdo.External.Uploader;
using Inscribe.Configuration;

namespace Inscribe.Plugin
{
    public static class UploaderManager
    {
        static List<IResolver> resolvers = new List<IResolver>();

        /// <summary>
        /// 画像URL リゾルバ
        /// </summary>
        public static IEnumerable<IResolver> Resolvers
        {
            get { return resolvers.AsReadOnly(); }
        }

        static List<IUploader> uploaders = new List<IUploader>();

        /// <summary>
        /// 画像アップローダ
        /// </summary>
        public static IEnumerable<IUploader> Uploaders
        {
            get { return uploaders.AsReadOnly(); }
        }

        /// <summary>
        /// アップローダを追加します。<para />
        /// リゾルバにも追加します。
        /// </summary>
        /// <param name="uploader">追加するアップローダインスタンス</param>
        public static void RegisterUploader(IUploader uploader)
        {
            if (!uploaders.Contains(uploader))
                uploaders.Add(uploader);
            RegisterResolver(uploader);
        }

        /// <summary>
        /// リゾルバを追加します。
        /// </summary>
        /// <param name="resolver">追加するリゾルバインスタンス</param>
        public static void RegisterResolver(IResolver resolver)
        {
            if (!resolvers.Contains(resolver))
                resolvers.Add(resolver);
        }

        /// <summary>
        /// 実行中のアセンブリに存在するすべてのリゾルバを列挙し、マネージャに追加します。
        /// </summary>
        public static void RegisterAllUploadersInAsm(Assembly asm)
        {

            foreach (var type in asm.GetTypes())
            {
                var ifs = type.GetInterfaces();
                if (ifs.Contains(typeof(IUploader)))
                {
                    RegisterUploader(Activator.CreateInstance(type) as IUploader);
                }
                else if (ifs.Contains(typeof(IResolver)))
                {
                    RegisterResolver(Activator.CreateInstance(type) as IResolver);
                }
            }
        }

        /// <summary>
        /// URLの解決を試みます。<para />
        /// 解決できない場合、nullを返します。
        /// </summary>
        /// <param name="url">画像URLの候補</param>
        /// <returns>解決できればtrue できなければnull</returns>
        public static string TryResolve(string url)
        {
            if (IsRawImageUrl(url)) return url;
            foreach (var resolv in resolvers)
            {
                if (resolv.IsResolvable(url))
                    return resolv.Resolve(url);
            }
            return null;
        }

        private static bool IsRawImageUrl(string url)
        {
            return String.IsNullOrWhiteSpace(url) ||
                url.EndsWith(".png") ||
                url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".jpe") ||
                url.EndsWith(".bmp") || url.EndsWith(".dib") ||
                url.EndsWith("gif");
        }

        public static IUploader GetSuggestedUploader()
        {
            string sn = Setting.Instance.ExternalProperty.UploaderService;
            if (String.IsNullOrEmpty(sn)) return null;
            foreach (var ss in uploaders)
            {
                if (ss.ServiceName == sn)
                {
                    return ss;
                }
            }
            return uploaders.FirstOrDefault();
        }
    }
}
