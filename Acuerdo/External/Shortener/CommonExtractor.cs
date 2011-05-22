using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Acuerdo.External.Shortener
{
    public class CommonExtractor : IURLExtractor
    {
        public bool TryDecompress(string url, out string decompressed)
        {
            decompressed = null;
            try
            {
                var res = GetReturnedURL(new Uri(url));
                if (!String.IsNullOrEmpty(url) && res.OriginalString != url)
                {
                    decompressed = res.OriginalString;
                    return true;
                }
                else
                    return false;
            }
            catch { return false; }
        }

        public Uri GetReturnedURL(Uri url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "HEAD";
            req.Timeout = 1500;
            HttpWebResponse res = null;

            try
            {
                res = (HttpWebResponse)req.GetResponse();
                var headloc = res.Headers["Location"];
                if (!String.IsNullOrEmpty(headloc))
                {
                    try
                    {
                        return new Uri(headloc);
                    }
                    catch { }
                }
                return res.ResponseUri;
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine("Request error at:" + url);
                res = (HttpWebResponse)ex.Response;

                if (res != null)
                {
                    return res.ResponseUri;
                }
                else
                {
                    throw; // サーバ接続不可などの場合は再スロー
                }
            }
            finally
            {
                if (res != null)
                {
                    res.Close();
                }
            }
        }
    }
}
