using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Inscribe;
using Inscribe.Configuration;
using Inscribe.Storage;

namespace Mystique.Update
{
    public static class UpdateReceiver
    {
        static Timer updateTimer;
        static bool updateReceived = false;

        public static void Start()
        {
            updateTimer = new Timer(CheckUpdate, null, 1000 * 10, 1000 * 60 * 60 * 3);
        }

        private static void CheckUpdate(object o)
        {
            try
            {
                if (updateReceived) return;
                updateReceived = CheckUpdate();
            }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.InternalError, "Krileの更新を確認できませんでした。", () => CheckUpdate());
            }
        }

        public static bool CheckUpdate()
        {
            var xmlstream = DownloadStream(new Uri(Define.RemoteVersionXml));
            if (xmlstream != null)
            {
                var xmldoc = XDocument.Load(xmlstream);
                var version = double.Parse(xmldoc.Element("update").Element("latests").Elements("version")
                    .Where(xe => int.Parse(xe.Attribute("kind").Value) <= Setting.Instance.ExperienceProperty.UpdateKind)
                    .Select(xe => xe.Attribute("ver").Value).FirstOrDefault());
                var pkey = xmldoc.Element("update").Element("pub").Value;
                CheckPublicKey(new Uri(pkey));
                var bin = xmldoc.Element("update").Element("bin").Value;
                var sig = xmldoc.Element("update").Element("sig").Value;
                if (version != 0 && version > Define.GetNumericVersion())
                {
                    Task.Factory.StartNew(() => DownloadUpdate(new Uri(bin), new Uri(sig)));
                    return true;
                }
            }
            return false;
        }

        private static void CheckPublicKey(Uri publicKey)
        {

        }

        private static void DownloadUpdate(Uri binary, Uri signature)
        {
        }


        private static Stream DownloadStream(Uri uri)
        {
            try
            {
                WebRequest request = HttpWebRequest.Create(uri);
                var ret = request.GetResponse();
                if (ret != null)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    using (var stream = ret.GetResponseStream())
                    {
                        byte[] buf = new byte[2048];
                        while (true)
                        {
                            int rlen = stream.Read(buf, 0, buf.Length);
                            if (rlen == 0) break;
                            memoryStream.Write(buf, 0, rlen);
                        }
                    }
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream;
                }
                return null;
            }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.InternalError, "更新確認に失敗しました", () => CheckUpdate());
                return null;
            }
        }
    }
}
