using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Vanille;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Security.Cryptography;

namespace Rouge
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = fbd.SelectedPath;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Krile Signatured Package|*.ksp";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    textBox2.Text = sfd.FileName;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using(var ofd = new OpenFileDialog())
            {
                ofd.Filter = "private key|*.prv";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBox3.Text = ofd.FileName;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string pubkey = null;
            string prvkey = null;
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "public key|*.pub";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    pubkey = sfd.FileName;
                }
            }
            if (pubkey == null) return;
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "private key|*.prv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    prvkey = sfd.FileName;
                }
            }
            if (prvkey == null) return;
            string pubkeybody;
            string prvkeybody;
            KASignature.CreateKeys(out pubkeybody, out prvkeybody);
            File.WriteAllText(pubkey, pubkeybody);
            File.WriteAllText(prvkey, prvkeybody);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!textBox1.Text.EndsWith("\\"))
                textBox1.AppendText("\\");
            label3.Text = "Collecting file information...";
            label3.Refresh();
            Application.DoEvents();
            List<FileCandidate> fc = new List<FileCandidate>();
            var files = Directory.GetFiles(textBox1.Text, "*.*", SearchOption.AllDirectories);
            int ctor = 0;
            foreach (var f in files)
            {
                ctor++;
                fc.Add(new FileCandidate(f));
            }
            var privkey = File.ReadAllText(textBox3.Text);
            label3.Text = "Writing file information...(" + ctor.ToString() + " files)";
            label3.Refresh();
            Application.DoEvents();
            MemoryStream writes = new MemoryStream();
            KrileArchive.Compress(writes, textBox1.Text, fc);
            writes.Seek(0, SeekOrigin.Begin);
            using (var fs = new FileStream(textBox2.Text, FileMode.Create, FileAccess.Write))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Compress))
                {
                    KASignature.Signature(writes, gz, privkey);
                }
            }
            using (var fs = new FileStream(textBox2.Text, FileMode.Open, FileAccess.Read))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                MemoryStream verified = new MemoryStream();
                if (!KASignature.Verify(gz, verified, privkey))
                    throw new SecurityException("Patch-pack verification failed.");
                verified.Seek(0, SeekOrigin.Begin);
                Directory.CreateDirectory(textBox2.Text + "__");
                KrileArchive.Extract(verified, textBox2.Text + "__");
            }
            label3.Text = "Completed!";
            label3.Refresh();
            Application.DoEvents();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            String openFile = String.Empty;
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "署名するファイルを選択";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    openFile = ofd.FileName;
                }
            }
            if (String.IsNullOrEmpty(openFile))
                return;
            String pkeyFile = String.Empty;
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "private key|*.prv";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pkeyFile = ofd.FileName;
                }
            }
            if (String.IsNullOrEmpty(pkeyFile))
                return;
            var write = openFile + ".sig";
            var sign = GetSignature(File.ReadAllBytes(openFile), File.ReadAllText(pkeyFile));
            File.WriteAllBytes(write, sign);
        }

        private byte[] GetSignature(byte[] bytes, String privateKey)
        {
            using (var sha = new SHA256Cng())
            using (var rsa = new RSACryptoServiceProvider())
            {
                // Compute hash
                var hash = sha.ComputeHash(bytes);
                // RSA Initialize
                rsa.FromXmlString(privateKey);
                // format
                var formatter = new RSAPKCS1SignatureFormatter(rsa);
                formatter.SetHashAlgorithm("SHA256");
                return formatter.CreateSignature(hash);
            }
        }
    }
}
