using System;
using System.Collections.Generic;

namespace Voices
{
    /// <summary>
    /// 自己診断
    /// </summary>
    public static class SelfAnalyzer
    {
        public static string Analyze(string error)
        {
            if (error.Contains("System.IO.DirectoryNotFoundException") ||
                error.Contains("System.IO.FileNotFoundException") ||
                error.Contains("System.TypeLoadException"))
            {
                return "Krileのファイル構成が壊れている可能性があります。" + Environment.NewLine +
                    "Krile 公式サイトより最新版をダウンロードして上書きしてみてください。" + Environment.NewLine +
                    "(ERR: FileOrDirNotFound)";
            }


            if (error.Contains("KrilePluginException"))
            {
                return "使用しているプラグインの互換性が無い可能性があります。" + Environment.NewLine +
                    "後から導入したプラグインをいったん削除して起動してみてください。" + Environment.NewLine +
                    "それでも起動しない場合は、Krileのファイル構成が壊れている可能性があります。" + Environment.NewLine +
                    "Krile 公式サイトより最新版をダウンロードして上書きしてみてください。" + Environment.NewLine +
                    "(ERR: PluginAsserted)";
            }
            if (error.Contains("System.MissingMethodException") ||
                error.Contains("System.MissingFieldException"))
            {
                return "使用しているプラグインの互換性が無い可能性があります。" + Environment.NewLine +
                    "後から導入したプラグインをいったん削除して起動してみてください。" + Environment.NewLine +
                    "それでも起動しない場合は、Krileのファイル構成が壊れている可能性があります。" + Environment.NewLine +
                    "Krile 公式サイトより最新版をダウンロードして上書きしてみてください。" + Environment.NewLine +
                    "(ERR: MissingFieldOrMethod)";
            }

            List<String> errors = new List<string>();
            if (error.Contains("MS.Internal.Documents.UndoManager.Close(IParentUndoUnit unit, UndoCloseAction closeAction)"))
                errors.Add("CloseAction");
            if (error.Contains("System.InvalidOperationException: Internal error: internal WPF code tried to reactivate a BindingExpression that was already marked as detached."))
                errors.Add("DetachFailed");
            if (error.Contains("System.Windows.FrameworkElement.ChangeLogicalParent(DependencyObject newParent)"))
                errors.Add("ChangeLogicalParent");
            if (error.Contains("System.Windows.Media.Composition.DUCE.Channel.SyncFlush()"))
                errors.Add("DUCE");
            if (errors.Count > 0)
            {
                return "Windows Presentation Foundation 内部エラーが発生しました。" + Environment.NewLine +
                    "お手数ですが再度起動してください。" + Environment.NewLine +
                    "(システムのメモリが不足している可能性があります。)" +
                    "(ERR: " + String.Join(", ", errors) + ")";
            }

            return null;
        }
    }
}
