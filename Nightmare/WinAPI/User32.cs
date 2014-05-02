using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nightmare.WinAPI
{
    public static class User32
    {
        /// <summary>
        /// 指定した仮想キーコードについてその状態を表した数値を返します。
        /// </summary>
        /// <param name="vKey">確認するキーの仮想キーコード</param>
        /// <returns>状態を表した数値</returns>
        [DllImport("User32.dll", SetLastError = true)]
        extern public static ushort GetAsyncKeyState([In] int vKey);

        /// <summary>
        /// キーが押された状態を表します。
        /// </summary>
        public static readonly ushort VK_STATE_PRESSING = (ushort)0x8000U;
        /// <summary>
        /// 以前の GetAsyncKeyState 関数の呼び出しの時にキーが押されていた状態を表します。
        /// </summary>
        public static readonly ushort VK_STATE_PRESSED = (ushort)0x1U;

        /// <summary>
        /// 指定した仮想キーコードに対応するキーが押下状態にあるか返します。
        /// GetAsyncKeyState 関数のヘルパー メソッドです。
        /// </summary>
        /// <param name="Key">確認するキーの仮想キーコード</param>
        /// <returns>キーが押下状態かどうか</returns>
        public static bool IsKeyPressed(VirtualKey Key)
        {
            return (GetAsyncKeyState((int)Key) & VK_STATE_PRESSING) != 0;
        }
    }

    /// <summary>
    /// 仮想キーコードの列挙体です。
    /// </summary>
    public enum VirtualKey : int
    {
        /// <summary>
        /// Shift キー (左右を区別しません)
        /// </summary>
        VK_SHIFT = 0x10,
        /// <summary>
        /// Ctrl キー (左右を区別しません)
        /// </summary>
        VK_CONTROL = 0x11
    }
}
