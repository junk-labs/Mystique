using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Xml.Linq;
using Inscribe.Configuration;
using Inscribe.Core;
using Inscribe.Storage;

namespace Inscribe.Common
{
    public static class UpdateReceiver
    {
        static Timer updateTimer;
        static bool updateReceived = false;

        public static void StartSchedule()
        {
            updateTimer = new Timer(CheckUpdate, null, 1000 * 10, 1000 * 60 * 60 * 3);
        }

        private static void CheckUpdate(object o)
        {
            try
            {
                if (updateReceived) return;
                updateReceived = CheckUpdate();
                if (updateReceived)
                    NotifyStorage.Notify("Krileは次回起動時に最新バージョンへ更新されます。");
            }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.InternalError, "Krileの更新を確認できませんでした。", () => CheckUpdate());
            }
        }

        public static bool CheckUpdate()
        {
            try
            {
                var xmlstream = DownloadStream(new Uri(Define.RemoteVersionXml));
                if (xmlstream != null)
                {
                    var xmldoc = XDocument.Load(xmlstream);
                    var version = xmldoc.Element("update").Element("latests").Elements("version")
                        .Where(xe => int.Parse(xe.Attribute("kind").Value) <= Setting.Instance.ExperienceProperty.UpdateKind)
                        .Select(xe => xe.Attribute("ver").Value).Select(double.Parse).Max();
                    // NO RECORD
                    if(version == 0)
                        return false;
                    var bin = xmldoc.Element("update").Element("bin").Value;
                    var sig = xmldoc.Element("update").Element("sig").Value;
                    if (version != 0 && version > Define.GetNumericVersion())
                    {
                        DownloadUpdate(new Uri(bin), new Uri(sig));
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex,
                    ExceptionCategory.InternalError,
                    "Krileの更新パッケージの確認中に問題が発生しました。");
                return false;
            }
        }

        private static void DownloadUpdate(Uri binary, Uri signature)
        {
            using (var binstream = DownloadStream(binary))
            using (var sigstream = DownloadStream(signature))
            {
                var binarray = binstream.ToArray();
                var sigarray = sigstream.ToArray();
                var pubkey = Path.Combine(Path.GetDirectoryName(Define.ExeFilePath), Define.PublicKeyFile);
                if (!File.Exists(pubkey))
                    throw new FileNotFoundException("パブリックキーが見つかりません。Krileを再インストールしてください。");
                if (VerifySignature(binarray, sigarray, File.ReadAllText(pubkey)))
                {
                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Define.ExeFilePath), Define.UpdateFileName), binarray);
                }
                else
                {
                    throw new SecurityException("アップデータの鍵が一致しません。手動でアップデートすることをお勧めします。");
                }
            }
        }

        private static bool VerifySignature(byte[] file, byte[] sign, String pubkey)
        {
            try
            {
                using (var sha = new SHA256Managed())
                using (var rsa = new RSACryptoServiceProvider())
                {
                    // Compute hash
                    var hash = sha.ComputeHash(file);
                    // RSA Initialize
                    rsa.FromXmlString(pubkey);
                    // deformat
                    var deformatter = new RSAPKCS1SignatureDeformatter(rsa);
                    deformatter.SetHashAlgorithm("SHA256");
                    return deformatter.VerifySignature(hash, sign);
                }
            }
            catch (SecurityException sex)
            {
                ExceptionStorage.Register(sex,
                     ExceptionCategory.InternalError,
                     "Krileの更新パッケージの署名を検証できません。手動更新してください。", () => CheckUpdate());
                return false;
            }
        }

        private static MemoryStream DownloadStream(Uri uri)
        {
            try
            {
                var request = HttpWebRequest.Create(uri);
                var ret = request.GetResponse();
                if (ret != null)
                {
                    var memoryStream = new MemoryStream();
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

        public static void StartUpdateArchive()
        {
            var path = Path.Combine(Path.GetDirectoryName(Define.ExeFilePath), Define.UpdateFileName);
            if (File.Exists(path))
            {
                // .completeファイルを作成する
                System.IO.File.Create(path + ".completed");
                Process.Start(
                    path,
                    Define.GetNumericVersion().ToString() + " " +
                    Setting.Instance.ExperienceProperty.UpdateKind.ToString() + " " +
                    Process.GetCurrentProcess().Id.ToString());
                KernelService.AppShutdown();
            }
        }
    }
}
