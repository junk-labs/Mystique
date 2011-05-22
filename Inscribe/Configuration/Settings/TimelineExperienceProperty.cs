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
            FastScrolling = true;
            ScrollLockMode = ScrollLock.OnSelected;
            IntelligentReordering = true;
            IntelligentReorderThreshold = 10;
            TimelineColorLazyUpdate = true;
            TimelineItemInitStrategy = ItemInitStrategy.DefaultColors;
            TimelineItemResetBreakCount = 50;
            TimelineItemResetBreakWait = 250;
        }

        public bool FastScrolling { get; set; }

        public enum ScrollLock
        {
            None,
            OnMouseCaptured,
            OnSelected,
            //OnScrolled
        }

        public ScrollLock ScrollLockMode { get; set; }

        /// <summary>
        /// TLに注目している間は、時間にかかわらず受信順にツイートを並べます。<para />
        /// (一定の期間より前に受信したツイートは時間通りに並べます。)
        /// </summary>
        public bool IntelligentReordering { get; set; }

        /// <summary>
        /// 現在の時刻からこれ以上前の分に投稿されたツイートについては、<para />
        /// Intelligent Reorderingを適用しません。
        /// </summary>
        public int IntelligentReorderThreshold { get; set; }

        /// <summary>
        /// タイムラインの色設定を遅延反映する(パフォーマンス向上、ただし大量に処理対象がある場合処理が遅延する場合あり)
        /// </summary>
        public bool TimelineColorLazyUpdate { get; set; }

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
}
