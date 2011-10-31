using System;

namespace Voices
{
    /// <summary>
    /// 自己診断
    /// </summary>
    public static class SelfAnalyzer
    {
        public static string Analyze(string error)
        {
            if (error.Contains("System.IO.DirectoryNotFoundException") || error.Contains("System.IO.FileNotFoundException"))
            {
                return "Krileのファイル構成が壊れている可能性があります。" + Environment.NewLine +
                    "Krile 公式サイトより最新版をダウンロードして上書きしてみてください。";
            }
            if (error.Contains("MS.Internal.Documents.UndoManager.Close(IParentUndoUnit unit, UndoCloseAction closeAction)") ||
                error.Contains("System.InvalidOperationException: Internal error: internal WPF code tried to reactivate a BindingExpression that was already marked as detached.") ||
                error.Contains("System.Windows.FrameworkElement.ChangeLogicalParent(DependencyObject newParent)") ||
                error.Contains("System.Windows.Media.Composition.DUCE.Channel.SyncFlush()"))
            {
                return "Windows Presentation Foundation 内部エラーが発生しました。" + Environment.NewLine +
                    "お手数ですが再度起動してください。" + Environment.NewLine +
                    "また、Windows Updateを確認してみてください。";
            }
            if (error.Contains("System.MissingMethodException"))
            {
                return "使用しているプラグインの互換性が無い可能性があります。" + Environment.NewLine +
                    "後から導入したプラグインをいったん削除して起動してみてください。" + Environment.NewLine +
                    "それでも起動しない場合は、Krileのファイル構成が壊れている可能性があります。" + Environment.NewLine +
                    "Krile 公式サイトより最新版をダウンロードして上書きしてみてください。";
            }
            return null;
        }
    }
}
