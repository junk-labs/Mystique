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
            KeywordSearchTransition = TransitionMethod.ViewStack;
            UserExtractTransition = TransitionMethod.AddTab;
            ConversationTransition = TransitionMethod.ViewStack;
            UserOpenTransition = TransitionMethod.AddTab;
            IsShowConversationTree = true;
            MoveUpToDeselect = true;
        }

        public bool FastScrolling { get; set; }

        public bool UseAscendingSort { get; set; }


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

        public TransitionMethod KeywordSearchTransition { get; set; }

        public TransitionMethod UserExtractTransition { get; set; }

        public TransitionMethod ConversationTransition { get; set; }

        public TransitionMethod UserOpenTransition { get; set; }

        /// <summary>
        /// 会話をツリーとして展開するか
        /// </summary>
        public bool IsShowConversationTree { get; set; }

        /// <summary>
        /// タイムラインの一番上の要素を選択している時にさらに上に移動しようとした時、選択を外す
        /// </summary>
        public bool MoveUpToDeselect { get; set; }

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

    public enum ScrollLock
    {
        // スクロールロックしない
        None,
        // マウスがリスト上に存在する時
        OnMouseCaptured,
        // 要素を選択している時
        OnSelected,
        // 常にスクロールロック
        Always,
    }
}
