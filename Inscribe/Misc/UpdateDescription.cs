using System;
using System.Collections.Generic;
using System.Linq;

namespace Inscribe.Misc
{
    public static class UpdateDescription
    {
        public static String GetUpdateDescription(double from, double to)
        {
            return GetUpdateDescriptions()
                .Where(t => t.Ver >= from && t.Ver <= to)
                .OrderBy(t => t.Ver)
                .Select(t => t.Desc)
                .JoinString(Environment.NewLine);
        }

        private static IEnumerable<T> GetUpdateDescriptions()
        {
            yield return new T(2003.9,
@"全文表示設定がオンの場合、閉じられないバグを修正。
規制予告アルゴリズムを焼きなおした。
更新通知ウィンドウ(このウィンドウです)を実装。");
            yield return new T(2003.8,
                @"オートコレクトが動作中に入力ボックスを閉じるアサインを実行した場合、オートコレクトを閉じる動作になるようにした。
縮小表示でも全文表示する設定を追加。(フルラインビューの置き換えです)
プラグインメニューを新設。KernelServiceから登録できます。
オートコレクトのサジェストにDPマッチングからの最尤一致を用いるようにした。
背景色にAlpha値を設定可能にした。
背景画像を表示できるようにした。(Alpha値を適切に設定する必要があります)
複数返信が指定できなくなるバグを修正。
タブクイックビルダーのテキスト抽出でフォーカスを移動させないとOKボタンが押せるようにならないバグを修正。");
        }

        private class T
        {
            public T(double ver, string desc)
            {
                this.Ver = ver;
                this.Desc = desc;
            }

            public double Ver { get; set; }
            public string Desc { get; set; }
        }
    }

}
