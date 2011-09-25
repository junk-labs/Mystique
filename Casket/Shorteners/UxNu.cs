using System;
using Acuerdo.External.Shortener;
using Dulcet.Network;
using Dulcet.Util;
using Inscribe.Storage;

namespace Casket.Shorteners
{
    /// <summary>
    /// ux.nu implementation
    /// </summary>
    public class UxNu : IUrlShortener, IUriExtractor
    {
        public bool IsCompressed(string url)
        {
            return url.StartsWith("http://ux.nu/");
        }

        public bool TryCompress(string url, out string compressed)
        {
            return TryCompressDecompress(url, out compressed, true);
        }

        public string Name
        {
            get { return "ux.nu"; }
        }

        public bool TryDecompress(string url, out string decompressed)
        {
            decompressed = null;
            return false;
            // return TryCompressDecompress(url, out decompressed, false);
        }

        private bool TryCompressDecompress(string url, out string result, bool compress)
        {
            try
            {
                string method = compress ? "short" : "hugeurl";
                var retd = Http.WebConnectDownloadString(
                    new Uri("http://ux.nu/api/" +
                        method +
                        "?url=" +
                        HttpUtility.UrlEncode(url) +
                        "&format=plain"));

                if (retd.Succeeded && !String.IsNullOrEmpty(retd.Data))
                {
                    result = retd.Data;
                    return true;
                }
            }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.PluginError,
                    "URLの圧縮/解除に失敗しました。");
            }
            result = null;
            return false;
        }
    }
}
