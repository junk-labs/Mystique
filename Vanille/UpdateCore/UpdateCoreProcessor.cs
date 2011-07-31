using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Vanille.UpdateCore
{
    public class UpdateCoreProcessor
    {
        public readonly double Version = 3.0;

        private Action<string> _writer;

        public UpdateCoreProcessor(Action<string> writer)
        {
            this._writer = writer;
        }

        public void Write(string text)
        {
            _writer(text);
        }

        public void WriteLine(string text)
        {
            _writer(text + Environment.NewLine);
        }

        private string desciptionXmlUrl = "http://krile.starwing.net/update/update.xml";

        private string publicKeyFile = "kup.pub";

        private List<String> removeFiles = new List<string>();

        public void BeginUpdate()
        {
            try
            {
                // Initialization
                WriteLine("--------------------------------------------------------------");
                WriteLine(" Welcome to Krile Update " + Version.ToString());
                WriteLine("--------------------------------------------------------------");
                SetCancellableInvoke(true);

                // Command parameters is
                // [File], [Version], [UpdateTarget], [Process ID], "runas"
                var cmd = Environment.GetCommandLineArgs();
                if (cmd.Length < 5)
                {
                    MessageBox.Show("Information losted.", "Updater", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Application.Exit();
                    return;
                }

                double origver = double.Parse(cmd[1]);
                int target = int.Parse(cmd[2]);
                int pid = int.Parse(cmd[3]);
                if (pid != 0)
                {
                    //プロセス終了を待つ
                    Write("(PID:" + pid.ToString() + ")...");
                    try
                    {
                        var p = Process.GetProcessById(pid);
                        for (int i = 0; i < 10 && !p.HasExited; i++)
                        {
                            p.WaitForExit(1000);
                            Write(".");
                        }
                        WriteLine("");

                        if (!p.HasExited)
                        {
                            //Krileがシャットダウンしない
                            WriteLine("<!>Krileがハングアップしている可能性があります。強制終了します...");
                            p.Kill();
                            Write("強制終了を待機しています...");
                            for (int i = 0; i < 10; i++)
                            {
                                Write(".");
                                p.WaitForExit(100);
                            }
                            WriteLine("");
                            if (p.HasExited)
                                WriteLine("Krileは終了されました。");
                            else
                            {
                                WriteLine("Krileを終了できませんでした。");
                                MessageBox.Show(
                                    "Krileを終了できないため、更新を継続できません。" + Environment.NewLine +
                                    "Windowsを再起動した後、手動での更新を試みてください。" + Environment.NewLine +
                                    "(エラーコード:00)",
                                    "更新エラー",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                Application.Exit();
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        WriteLine("::プロセス情報を取得できません。Krileはすでに終了しています。");
                    }
                }
                else
                {
                    WriteLine("(プロセスID不明)");
#if !DEBUG
                    MessageBox.Show(
                        "Krileを終了できないため、更新を継続できません。" + Environment.NewLine +
                        "Windowsを再起動した後、手動での更新を試みてください。" + Environment.NewLine +
                        "(エラーコード:02)",
                        "更新エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
#endif
                }

                // Start Update

                WriteLine("Downloading Patch Information:");
                WriteLine(" from " + desciptionXmlUrl + " ...");

                var xmlstream = DownloadStream(new Uri(desciptionXmlUrl));
                if(xmlstream == null)
                {
                    WriteLine("ERR: Description download failed!");
                    return;
                }

                WriteLine("Downloaded.");

                var xmldoc = XDocument.Load(xmlstream);
                var version = double.Parse(xmldoc.Element("update").Element("latests").Elements("version")
                    .Where(xe => int.Parse(xe.Attribute("kind").Value) <= target)
                    .Select(xe => xe.Attribute("ver").Value).FirstOrDefault());
                if (version == 0)
                {
                    WriteLine("ERR: New version file is not found!");
                    MessageBox.Show("新しいバージョンのKrileを見つけられませんでした。",
                        "更新エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                WriteLine("Krile update from " + origver + " to " + version + "...");

                SequenceApplyPatches(
                    SequenceDeterminePatches(xmldoc.Element("update").Element("patches"), target, origver));
            }
            finally
            {
                // cleanup files
                foreach (var path in removeFiles)
                {
                    File.Delete(path);
                }
            }
        }

        public IEnumerable<string> SequenceDeterminePatches(XElement node, int downloadSwitch, double origver)
        {
            return
                node.Elements("patch")
                    .Where(e => int.Parse(e.Attribute("kind").Value) <= downloadSwitch &&
                        double.Parse(e.Attribute("ver").Value) >= origver)
                    .OrderBy(e => double.Parse(e.Attribute("ver").Value))
                    .Select(e => e.Value);
        }

        public void SequenceApplyPatches(IEnumerable<string> uris)
        {
            SetCancellableInvoke(false);
            WriteLine("Starting patch...");
            var pubkey = File.ReadAllText(Path.Combine(Application.StartupPath, publicKeyFile));
            foreach (var uri in uris)
            {
                WriteLine("Downloading patch-pack " + uri + "...");
                var dlstream = DownloadStream(new Uri(uri));
                if (dlstream == null)
                    throw new Exception("File download error: " + uri);
                string dest = null;
                while (dest == null || Directory.Exists(dest) || File.Exists(dest))
                {
                    dest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                }
                Directory.CreateDirectory(dest);
                try
                {
                    using (var gz = new GZipStream(dlstream, CompressionMode.Decompress))
                    {
                        WriteLine("Verifying patch-pack...");
                        MemoryStream verified = new MemoryStream();
                        if (!KASignature.Verify(gz, verified, pubkey))
                            throw new SecurityException("Patch-pack verification failed.");
                        WriteLine("Extracting patch-pack...");
                        verified.Seek(0, SeekOrigin.Begin);
                        KrileArchive.Extract(verified, dest);
                        WriteLine("Updating files...");
                        MoveDirectory(dest, Application.StartupPath, true);
                        WriteLine("Complete.");
                    }
                }
                finally
                {
                    WriteLine("Removing temporally files...");
                    Directory.Delete(dest, true);
                }
            }
            WriteLine("All Done!");
        }

        #region Utilities

        private Stream DownloadStream(Uri uri)
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
                WriteLine("Download Error: " + e.Message + " --------------------------");
                WriteLine(e.ToString());
                return null;
            }
        }

        public void MoveDirectory(string source, string dest, bool overwritable)
        {
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
                File.SetAttributes(dest, File.GetAttributes(source));
                overwritable = true;
            }

            foreach (string cfrom in Directory.GetFiles(source))
            {
                string cdest = Path.Combine(dest, Path.GetFileName(cfrom));

                if (File.Exists(cdest))
                {
                    if (overwritable)
                    {
                    FileDelete:
                        try
                        {
                            File.Delete(cdest);
                        }
                        catch (IOException)
                        {
                            var ret = MessageBox.Show(
                                "ファイルを削除できません:" + Environment.NewLine +
                                cdest,
                                "削除エラー", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                            switch (ret)
                            {
                                case DialogResult.Ignore:
                                    break;
                                case DialogResult.Retry:
                                    goto FileDelete;
                                case DialogResult.Abort:
                                    throw new Exception("Aborted.");
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                File.Move(cfrom, cdest);
            }

            //recursive
            foreach (string copyFrom in Directory.GetDirectories(source))
            {
                string copyTo = Path.Combine(dest, Path.GetFileName(copyFrom));
                MoveDirectory(copyFrom, copyTo, overwritable);
            }
        }

        public event Action<bool> SetCancellable;
        private void SetCancellableInvoke(bool cancellable)
        {
            if (SetCancellable != null)
                SetCancellable.Invoke(cancellable);
        }
        #endregion
    }
}
