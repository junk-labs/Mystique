using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace Nightmare.Forms
{
    /// <summary>
    /// System.Windows.Forms.Iconのラッパ実装です。
    /// </summary>
    public class WinFormsIcon
    {
        private Icon winFormsIcon;
        internal Icon IconInstance { get { return this.winFormsIcon; } }

        public WinFormsIcon(Stream stream)
        {
            this.winFormsIcon = new Icon(stream);
        }

        public WinFormsIcon(string file)
        {
            this.winFormsIcon = new Icon(file);
        }

        internal WinFormsIcon(Icon icon)
        {
            this.winFormsIcon = icon;
        }
    }
}
