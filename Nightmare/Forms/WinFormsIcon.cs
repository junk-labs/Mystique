using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

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

        public WinFormsIcon(BitmapImage image)
        {
            Bitmap bitmap = null;
            var width = image.PixelWidth;
            var height = image.PixelHeight;
            var stride = width * ((image.Format.BitsPerPixel + 7) / 8);
            var bits = new byte[height * stride];
            unsafe
            {
                fixed (byte* pB = bits)
                {
                    var ptr = new IntPtr(pB);
                    bitmap = new Bitmap(width, height, stride,
                        System.Drawing.Imaging.PixelFormat.Format32bppPArgb,
                        ptr);
                }
            }
            this.winFormsIcon = Icon.FromHandle(bitmap.GetHicon());
        }

        internal WinFormsIcon(Icon icon)
        {
            this.winFormsIcon = icon;
        }

    }
}
