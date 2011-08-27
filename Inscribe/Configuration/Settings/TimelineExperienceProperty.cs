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
            this.OrderByAscending = false;
            UseFastScrolling = true;
            ScrollLockMode = ScrollLock.OnSelected;
            QueryApplyWait = 200;
            KeywordSearchTransition = TransitionMethod.ViewStack;
            UserExtractTransition = TransitionMethod.AddTab;
            ConversationTransition = TransitionMethod.ViewStack;
            UserOpenTransition = TransitionMethod.AddTab;
            IsShowConversationAsTree = true;
            MoveAboveTopToDeselect = true;
            UseIntelligentOrdering = false;
            IntelligentOrderingThresholdSec = 60 * 5;
        }

        public bool UseFastScrolling { get; set; }

        public bool OrderByAscending { get; set; }

        public ItemInitStrategy TimelineItemInitStrategy { get; set; }

        public ScrollLock ScrollLockMode { get; set; }

        public int QueryApplyWait;

        public TransitionMethod KeywordSearchTransition { get; set; }

        public TransitionMethod UserExtractTransition { get; set; }

        public TransitionMethod ConversationTransition { get; set; }

        public TransitionMethod UserOpenTransition { get; set; }

        /// <summary>
        /// 会話をツリーとして展開するか
        /// </summary>
        public bool IsShowConversationAsTree { get; set; }

        /// <summary>
        /// タイムラインの一番上の要素を選択している時にさらに上に移動しようとした時、選択を外す
        /// </summary>
        public bool MoveAboveTopToDeselect { get; set; }

        /// <summary>
        /// 受信順を投稿順よりも優先
        /// </summary>
        public bool UseIntelligentOrdering { get; set; }

        public int IntelligentOrderingThresholdSec { get; set; }
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
