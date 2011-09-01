using System;
using System.IO;
using Inscribe.Core;
using Inscribe.ViewModels.PartBlocks.MainBlock;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;

namespace Inscribe.Common
{
    public static class KeyAssignHelper
    {
        /// <summary>
        /// キーアサイン ファイルへのフルパスを取得します。
        /// </summary>
        /// <param name="fileName">アサインファイル名</param>
        public static String GetPath(string fileName)
        {
            return Path.Combine(Path.GetDirectoryName(Define.ExeFilePath), Define.KeyAssignDirectory, fileName);
        }

        /// <summary>
        /// 選択中のTabViewModelに依存するアクションを実行します。<para />
        /// 選択タブが無い場合は何も行いません。
        /// </summary>
        public static void ExecuteTabAction(Action<TabViewModel> action)
        {
            var ctab = KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentTab;
            if (ctab != null)
                action(ctab);
        }

        /// <summary>
        /// 選択中のTabDependentTweetViewModelに依存するアクションを実行します。<para />
        /// 選択ツイートが無い場合は何も行いません。
        /// </summary>
        public static void ExecuteTVMAction(Action<TabDependentTweetViewModel> action)
        {
            var ctvm = KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentTab;
            var cc = ctvm != null ? ctvm.CurrentForegroundTimeline : null;
            if (cc == null) return;
            var vm = cc.SelectedTweetViewModel;
            if (vm == null) return;
            action(vm);
        }
    }
}
