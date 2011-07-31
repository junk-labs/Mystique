using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Vanille
{
    public static class KrileArchive
    {
        public static void Extract(Stream input, string outdir)
        {
            byte[] buffer = new byte[1024];
            List<FileData> fd = new List<FileData>();
            if (input.Read(buffer, 0, 256) < 256)
                throw new Exception("File is corrupted.(KAR: Magic Not Found)");
            var magicstr = Encoding.UTF8.GetString(buffer).Split(new[] { ":" }, StringSplitOptions.None);
            if (magicstr.Length < 3)
                throw new Exception("File is corrupted.(KAR: Invalid Magic Length)");
            if (magicstr[0] != "KAR")
                throw new Exception("File is corrupted.(KAR: Invalid Magic String)");
            int arcnum = int.Parse(magicstr[1]);
            long desclen = long.Parse(magicstr[2]);
            using (MemoryStream ms = new MemoryStream())
            {
                ReadStream(input, ms, desclen);
                var fds = Encoding.UTF8.GetString(ms.ToArray()).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var f in fds)
                {
                    var sfd = f.Split(new[] { "?" }, StringSplitOptions.None);
                    if (sfd.Length != 2)
                        throw new Exception("File is corrupted.(KAR: Invalid File Description)");
                    fd.Add(new FileData(sfd[0], long.Parse(sfd[1])));
                }
                if (fd.Count != arcnum)
                    throw new Exception("File is corrupted.(KAR: File Count Mismatched)");

                foreach (var fdi in fd)
                {
                    var tgfp = Path.Combine(outdir, fdi.Name);
                    if (!Directory.Exists(Path.GetDirectoryName(tgfp)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(tgfp));
                    }
                    using (var cfs = new FileStream(tgfp, FileMode.Create, FileAccess.ReadWrite))
                    {
                        ReadStream(input, cfs, fdi.Length);
                    }
                }
            }
        }

        public static void Compress(Stream output, string rootdir, IEnumerable<FileCandidate> files)
        {
            StringBuilder descs = new StringBuilder();
            foreach (var fcd in files)
            {
                descs.AppendLine(fcd.GetRelativePath(rootdir) + "?" + fcd.GetFileLength());
            }
            var descbytes = Encoding.UTF8.GetBytes(descs.ToString());
            string magic = "KAR:" + files.Count() + ":" + descbytes.Length.ToString() + ":";
            var magicbytes = Encoding.UTF8.GetBytes(magic);
            if (magicbytes.Length > 256)
                throw new Exception("Archiving error:Too long");
            output.Write(magicbytes, 0, magicbytes.Length);
            byte[] padding = new byte[256 - magicbytes.Length];
            output.Write(padding, 0, padding.Length);
            output.Write(descbytes, 0, descbytes.Length);
            int lc = 0;
            foreach (var fcd in files)
            {
                lc++;
                using (var infs = new FileStream(fcd.path, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[1024];
                    while (true)
                    {
                        //圧縮するファイルからデータを読み込む
                        int readSize = infs.Read(buffer, 0, buffer.Length);
                        //最後まで読み込んだ時は、ループを抜ける
                        if (readSize == 0)
                            break;
                        //データを圧縮して書き込む
                        output.Write(buffer, 0, readSize);
                    }
                }
            }
        }

        private static void ReadStream(Stream ins, Stream outs, long length)
        {
            byte[] buffer = new byte[1024];
            long readedLen = 0;
            while (true)
            {
                long nextRead = length - readedLen;
                if (nextRead > 1024)
                    nextRead = 1024;
                //書庫から展開されたデータを読み込む
                int readSize = ins.Read(buffer, 0, (int)nextRead);
                readedLen += readSize;
                //最後まで読み込んだ時は、ループを抜ける
                if (readSize == 0)
                    return;
                //出力に書き込む
                outs.Write(buffer, 0, readSize);
                if (readedLen >= length)
                    return;
            }
        }

        public static string GetFileHash(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var md5 = new MD5CryptoServiceProvider();
                try
                {
                    var ret = md5.ComputeHash(fs);
                    return BitConverter.ToString(ret).ToLower().Replace("-", "");
                }
                finally
                {
                    md5.Clear();
                }
            }
        }

        private struct FileData
        {
            public FileData(string name, long length)
            {
                this.Name = name;
                this.Length = length;
            }
            public string Name;
            public long Length;
        }
    }

    public class FileCandidate
    {
        public string path;

        public FileCandidate(string path)
        {
            this.path = path;
        }

        public string GetRelativePath(string basedir)
        {
            string bdc = basedir;
            try
            {
                while (!path.StartsWith(bdc))
                {
                    bdc = Path.GetDirectoryName(bdc);
                    if (bdc == String.Empty)
                        break;
                }
            }
            catch { bdc = String.Empty; }
            var bdr = basedir.Substring(bdc.Length);
            var cpr = path.Substring(bdc.Length);
            return String.Join("/", Enumerable.Repeat("..", bdr.Count(c => c == '\\'))) + cpr;
        }

        public long GetFileLength()
        {
            var fi = new FileInfo(path);
            return fi.Length;
        }
    }
}
