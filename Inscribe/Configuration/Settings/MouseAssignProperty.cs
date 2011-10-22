
namespace Inscribe.Configuration.Settings
{
    /// <summary>
    /// マウスアサイン
    /// </summary>
    public class MouseAssignProperty
    {
        private ActionSet<ReplyMouseActionCandidates> replyActionSet;
        public ActionSet<ReplyMouseActionCandidates> ReplyActionSet
        {
            get { return replyActionSet ?? new ActionSet<ReplyMouseActionCandidates>(); }
            set { replyActionSet = value; }
        }

        private ActionSet<FavMouseActionCandidates> favActionSet;
        public ActionSet<FavMouseActionCandidates> FavActionSet
        {
            get { return favActionSet ?? new ActionSet<FavMouseActionCandidates>(); }
            set { favActionSet = value; }
        }

        private ActionSet<RetweetMouseActionCandidates> retweetActionSet;
        public ActionSet<RetweetMouseActionCandidates> RetweetActionSet
        {
            get { return retweetActionSet ?? new ActionSet<RetweetMouseActionCandidates>(); }
            set { retweetActionSet = value; }
        }

        private ActionSet<UnofficialRetweetQuoteMouseActionCandidates> unofficalRetweetActionSet;
        public ActionSet<UnofficialRetweetQuoteMouseActionCandidates> UnofficalRetweetActionSet
        {
            get { return unofficalRetweetActionSet ?? new ActionSet<UnofficialRetweetQuoteMouseActionCandidates>(); }
            set { unofficalRetweetActionSet = value; }
        }

        private ActionSet<UnofficialRetweetQuoteMouseActionCandidates> quoteTweetActionSet;
        public ActionSet<UnofficialRetweetQuoteMouseActionCandidates> QuoteTweetActionSet
        {
            get { return quoteTweetActionSet ?? new ActionSet<UnofficialRetweetQuoteMouseActionCandidates>(); }
            set { quoteTweetActionSet = value; }
        }
    }

    public class ActionSet<T>
    {
        public ActionDescription<T> NoneKeyAction { get; set; }
        public ActionDescription<T> ControlKeyAction { get; set; }
        public ActionDescription<T> AltKeyAction { get; set; }
        public ActionDescription<T> ShiftKeyAction { get; set; }
    }

    public class ActionDescription<T>
    {
        public T Action { get; set; }
        public string ActionArgs { get; set; }
    }

    public enum ReplyMouseActionCandidates
    {
        Reply,
        ReplyFromSpecificAccount,
        ReplyImmediately,
    }

    public enum FavMouseActionCandidates
    {
        FavToggle,
        FavAdd,
        FavRemove,
        FavSelect,
        FavToggleWithSpecificAccount,
        FavAddWithSpecificAccount,
        FavRemoveWithSpecificAccount,
        FavAddWithAllAccount,
        FavRemoveWithAllAccount,
    }

    public enum RetweetMouseActionCandidates
    {
        RetweetToggle,
        RetweetAdd,
        RetweetRemove,
        RetweetSelect,
        RetweetToggleWithSpecificAccount,
        RetweetAddWithSpecificAccount,
        RetweetRemoveWithSpecificAccount,
        RetweetAddWithAllAccount,
        RetweetRemoveWithAllAccount,
    }

    public enum UnofficialRetweetQuoteMouseActionCandidates
    {
        DefaultUnofficialRetweet,
        DefaultQuoteTweet,
        CustomUnofficialRetweet,
        CustomQuoteTweet,
        CustomUnofficialRetweetImmediately,
        CustomQuoteTweetImmediately,
    }
}
