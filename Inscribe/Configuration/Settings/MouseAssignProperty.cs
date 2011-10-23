
namespace Inscribe.Configuration.Settings
{
    /// <summary>
    /// マウスアサイン
    /// </summary>
    public class MouseAssignProperty
    {
        public MouseAssignProperty()
        {
            isMouseAssignEnabled = false;
            replyActionSet = new ActionSet<ReplyMouseActionCandidates>();
            replyActionSet.NoneKeyAction = new ActionDescription<ReplyMouseActionCandidates>(ReplyMouseActionCandidates.Reply);
            favActionSet = new ActionSet<FavMouseActionCandidates>();
            favActionSet.NoneKeyAction = new ActionDescription<FavMouseActionCandidates>(FavMouseActionCandidates.FavToggle);
            favActionSet.ShiftKeyAction = new ActionDescription<FavMouseActionCandidates>(FavMouseActionCandidates.FavSelect);
            retweetActionSet = new ActionSet<RetweetMouseActionCandidates>();
            retweetActionSet.NoneKeyAction = new ActionDescription<RetweetMouseActionCandidates>(RetweetMouseActionCandidates.RetweetToggle);
            retweetActionSet.ShiftKeyAction = new ActionDescription<RetweetMouseActionCandidates>(RetweetMouseActionCandidates.RetweetSelect);
            unofficialRetweetActionSet = new ActionSet<UnofficialRetweetQuoteMouseActionCandidates>();
            unofficialRetweetActionSet.NoneKeyAction = new ActionDescription<UnofficialRetweetQuoteMouseActionCandidates>(UnofficialRetweetQuoteMouseActionCandidates.DefaultUnofficialRetweet);
            quoteTweetActionSet = new ActionSet<UnofficialRetweetQuoteMouseActionCandidates>();
            quoteTweetActionSet.NoneKeyAction = new ActionDescription<UnofficialRetweetQuoteMouseActionCandidates>(UnofficialRetweetQuoteMouseActionCandidates.DefaultQuoteTweet);
        }

        private bool isMouseAssignEnabled;
        public bool IsMouseAssignEnabled
        {
            get { return isMouseAssignEnabled; }
            set { isMouseAssignEnabled = value; }
        }

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

        private ActionSet<UnofficialRetweetQuoteMouseActionCandidates> unofficialRetweetActionSet;
        public ActionSet<UnofficialRetweetQuoteMouseActionCandidates> UnofficialRetweetActionSet
        {
            get { return unofficialRetweetActionSet ?? new ActionSet<UnofficialRetweetQuoteMouseActionCandidates>(); }
            set { unofficialRetweetActionSet = value; }
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
        public ActionDescription() { }
        public ActionDescription(T action) : this(action, null) { }
        public ActionDescription(T action, string args)
        {
            this.Action = action;
            this.ActionArgs = args;
        }
        public T Action { get; set; }
        public string ActionArgs { get; set; }
    }

    public enum ReplyMouseActionCandidates
    {
        None,
        Reply,
        ReplyFromSpecificAccount,
        ReplyImmediately,
    }

    public enum FavMouseActionCandidates
    {
        None,
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
        None,
        RetweetToggle,
        RetweetCreate,
        RetweetDelete,
        RetweetSelect,
        RetweetToggleWithSpecificAccount,
        RetweetCreateWithSpecificAccount,
        RetweetDeleteWithSpecificAccount,
        RetweetCreateWithAllAccount,
        RetweetDeleteWithAllAccount,
    }

    public enum UnofficialRetweetQuoteMouseActionCandidates
    {
        None,
        DefaultUnofficialRetweet,
        DefaultQuoteTweet,
        CustomUnofficialRetweet,
        CustomQuoteTweet,
        CustomUnofficialRetweetImmediately,
        CustomQuoteTweetImmediately,
    }
}
