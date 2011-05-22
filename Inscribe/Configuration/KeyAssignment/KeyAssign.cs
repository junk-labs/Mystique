using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Configuration.KeyAssignment
{
    /// <summary>
    /// キーアサインの管理を行います。
    /// </summary>
    public static class KeyAssign
    {
        private static Dictionary<string, Action> callbacks;

        private static bool isInitalizing = false;

        static KeyAssign()
        {
            callbacks = new Dictionary<string, Action>();
        }

        public static void BeginInitialize()
        {
            isInitalizing = true;
        }

        public static void Register(Action method, string identifier)
        {
            if (!isInitalizing)
                throw new InvalidOperationException("初期化状態にないため、メソッド登録を受け付けられません。");
            callbacks.Add(identifier, method);
        }

        public static void EndInitialize()
        {
            isInitalizing = false;
        }
    }
}
