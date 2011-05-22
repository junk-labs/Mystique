using System;
using System.Linq;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace Nightmare.Forms
{
    public class Screen
    {
        /// <summary>
        /// 全てのスクリーンを取得します。
        /// </summary>
        public static Screen[] AllScreens
        {
            get
            {
                var all = WinForms.Screen.AllScreens;
                if (all == null)
                    return null;
                else
                    return all.Select(s => Wrap(s)).ToArray();
            }
        }

        /// <summary>
        /// プライマリ スクリーンを取得します。
        /// </summary>
        public static Screen PrimaryScreen
        {
            get { return Wrap(WinForms.Screen.PrimaryScreen); }
        }

        /// <summary>
        /// 指定したハンドルを持つコントロールが存在するスクリーンを取得します。
        /// </summary>
        /// <param name="handle">ハンドル</param>
        public static Screen FromHandle(IntPtr handle)
        {
            return Wrap(WinForms.Screen.FromHandle(handle));
        }

        /// <summary>
        /// 指定したポイントがあるスクリーンを取得します。<para />
        /// それぞれの値はintに丸められます。
        /// </summary>
        public static Screen FromPoint(Point pt)
        {
            return Wrap(WinForms.Screen.FromPoint(new System.Drawing.Point((int)pt.X, (int)pt.Y)));
        }

        private static Screen Wrap(WinForms.Screen screen)
        {
            if (screen == null)
                return null;
            else
                return new Screen(screen);
        }

        readonly WinForms.Screen original;

        private Screen(WinForms.Screen wfScreen)
        {
            if (wfScreen == null)
                throw new ArgumentNullException("wfScreen");
            this.original = wfScreen;
        }

        /// <summary>
        /// 1ピクセルに関連付けられているメモリのビット数を示します。
        /// </summary>
        public int BitsPerPixel
        {
            get { return this.original.BitsPerPixel; }
        }

        /// <summary>
        /// ディスプレイ領域を取得します。
        /// </summary>
        public Rect Bounds
        {
            get
            {
                return new Rect(
                      this.original.Bounds.Left,
                      this.original.Bounds.Top,
                      this.original.Bounds.Width,
                      this.original.Bounds.Bottom);
            }
        }

        /// <summary>
        /// このスクリーンの作業領域を取得します。
        /// </summary>
        public Rect WorkingArea
        {
            get
            {
                return new Rect(
                      this.original.WorkingArea.Left,
                      this.original.WorkingArea.Top,
                      this.original.WorkingArea.Width,
                      this.original.WorkingArea.Bottom);
            }
        }

        /// <summary>
        /// デバイス名を取得します。
        /// </summary>
        public string DeviceName
        {
            get { return this.original.DeviceName; }
        }

        /// <summary>
        /// このスクリーンがプライマリスクリーンかどうかを取得します。
        /// </summary>
        public bool IsPrimary
        {
            get { return this.original.Primary; }
        }
    }
}
