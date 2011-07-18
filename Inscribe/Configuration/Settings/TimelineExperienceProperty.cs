using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Configuration.Settings
{
    public class TimelineExperienceProperty
    {
        public TimelineExperienceProperty()
        {
            this.UseAscendingSort = false;
            FastScrolling = true;
            ScrollLockMode = ScrollLock.OnSelected;
            QueryApplyWait = 200;
            TimelineColorLazyUpdate = true;
            TimelineItemInitStrategy = ItemInitStrategy.DefaultColors;
            TimelineItemResetBreakCount = 50;
            TimelineItemResetBreakWait = 250;
        }

        public bool FastScrolling { get; set; }

        public bool UseAscendingSort { get; set; }

        public enum ScrollLock
        {
            None,
            OnMouseCaptured,
            OnSelected,
            //OnScrolled
        }

        public ScrollLock ScrollLockMode { get; set; }

        public int QueryApplyWait;

        /// <summary>
        /// タイムラインの色設定を遅延反映する(パフォーマンス向上、ただし大量に処理対象がある場合処理が遅延する場合あり)
        /// </summary>
        public bool TimelineColorLazyUpdate { get; set; }


        /// <summary>
        /// タイムラインの各要素の初期化方法
        /// </summary>
        public ItemInitStrategy TimelineItemInitStrategy { get; set; }

        /// <summary>
        /// タイムラインの要素を一気に追加する際、一時的に休止するカウント数
        /// </summary>
        public int TimelineItemResetBreakCount { get; set; }

        /// <summary>
        /// タイムラインの要素を一気に追加する際、一時的に休止する長さ
        /// </summary>
        public int TimelineItemResetBreakWait { get; set; }

    }

    /// <summary>
    /// タイムラインアイテムの初期化ストラテジ
    /// </summary>
    public enum ItemInitStrategy
    {
        /// <summary>
        /// 色を設定しません(最速)
        /// </summary>
        None,
        /// <summary>
        /// 色を常にデフォルトに設定します(普通)
        /// </summary>
        DefaultColors,
        /// <summary>
        /// 色を完全に設定します(低速)
        /// </summary>
        Full
    }

    /// <summary>
    /// 画面遷移の方法
    /// </summary>
    public enum TransitionMethod
    {
        ViewStack,
        AddTab,
        AddColumn
    }
}
