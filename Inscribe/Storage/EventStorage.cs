using System;
using System.Collections.Generic;
using System.Threading;
using Inscribe.Util;
using Inscribe.ViewModels.PartBlocks.MainBlock;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;
using Inscribe.Common;
using Dulcet.Twitter;
using Inscribe.Configuration;

namespace Inscribe.Storage
{
    public static class EventStorage
    {
        static EventStorage()
        {
            InitInvisibleSomething();
        }

        static DateTime wakeupTime = DateTime.Now;

        private static SafeLinkedList<EventDescription> events = new SafeLinkedList<EventDescription>();

        public static IEnumerable<EventDescription> Events
        {
            get { return events; }
        }

        public static void Remove(EventDescription evd)
        {
            events.Remove(evd);
            OnEventChanged(EventArgs.Empty);
        }

        private static void Register(EventDescription description, bool enforceRaise = false)
        {
            // check is muted
            if ((description.TargetTweet != null && FilterHelper.IsMuted(description.TargetTweet.Status)) ||
                (description.SourceUser != null && FilterHelper.IsMuted(description.SourceUser.TwitterUser)) ||
                (description.TargetUser != null && FilterHelper.IsMuted(description.TargetUser.TwitterUser)))
                return;
            events.AddLast(description);
            OnEventChanged(EventArgs.Empty);
            if (enforceRaise ||
                !Setting.Instance.NotificationProperty.IsInvisibleSomethingEnabled || !IsBlacklisted(description.SourceUser.TwitterUser))
                OnEventRegistered(new EventDescriptionEventArgs(description));
        }

        #region EventRegisteredイベント

        public static event EventHandler<EventDescriptionEventArgs> EventRegistered;
        private static Notificator<EventDescriptionEventArgs> _EventRegisteredEvent;
        public static Notificator<EventDescriptionEventArgs> EventRegisteredEvent
        {
            get
            {
                if (_EventRegisteredEvent == null) _EventRegisteredEvent = new Notificator<EventDescriptionEventArgs>();
                return _EventRegisteredEvent;
            }
            set { _EventRegisteredEvent = value; }
        }

        private static void OnEventRegistered(EventDescriptionEventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref EventRegistered, null, null);
            if (threadSafeHandler != null) threadSafeHandler(null, e);
            EventRegisteredEvent.Raise(e);
        }

        #endregion
      
        #region EventChangedイベント

        public static event EventHandler<EventArgs> EventChanged;
        private static Notificator<EventArgs> _EventChangedEvent;
        public static Notificator<EventArgs> EventChangedEvent
        {
            get
            {
                if (_EventChangedEvent == null) _EventChangedEvent = new Notificator<EventArgs>();
                return _EventChangedEvent;
            }
            set { _EventChangedEvent = value; }
        }

        private static void OnEventChanged(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref EventChanged, null, null);
            if (threadSafeHandler != null) threadSafeHandler(null, e);
            EventChangedEvent.Raise(e);
        }

        #endregion

        public static void OnRetweeted(TweetViewModel tweet, UserViewModel retweeter)
        {
            if (AccountStorage.Contains(retweeter.TwitterUser.ScreenName) || tweet.CreatedAt < wakeupTime)
                return;
            Register(new EventDescription(EventKind.Retweet, retweeter,
                    UserStorage.Get(tweet.Status.User), tweet));
        }

        public static void OnFavored(TweetViewModel tweet, UserViewModel favorer)
        {
            if (AccountStorage.Contains(favorer.TwitterUser.ScreenName))
                return;
            Register(new EventDescription(EventKind.Favorite, favorer,
                 UserStorage.Get(tweet.Status.User), tweet));
        }

        public static void OnUnfavored(TweetViewModel tweet, UserViewModel favorer)
        {
            if (AccountStorage.Contains(favorer.TwitterUser.ScreenName))
                return;
            Register(new EventDescription(EventKind.Unfavorite,
                favorer, UserStorage.Get(tweet.Status.User), tweet));
        }

        public static void OnFollowed(UserViewModel fromUser, UserViewModel toUser)
        {
            Register(new EventDescription(EventKind.Follow,
                fromUser, toUser));
        }

        public static void OnRemoved(UserViewModel fromUser, UserViewModel toUser)
        {
            Register(new EventDescription(EventKind.Unfollow,
                fromUser, toUser));
        }

        public static void OnMention(TweetViewModel tweet)
        {
            if (AccountStorage.Contains(tweet.Status.User.ScreenName) || tweet.CreatedAt < wakeupTime)
                return;
            Register(new EventDescription(EventKind.Mention,
                UserStorage.Get(tweet.Status.User), null, tweet));
        }

        public static void OnDirectMessage(TweetViewModel tweet)
        {
            if (AccountStorage.Contains(tweet.Status.User.ScreenName) || tweet.CreatedAt < wakeupTime)
                return;
            Register(new EventDescription(EventKind.DirectMessage,
                UserStorage.Get(tweet.Status.User), null, tweet));
        }

        #region InvisibleSomething

        // インビジブルなんとかの実装

        private static object listLock = new object();

        const int BLACKLIST_THRESHOLD = 3;

        private static SortedDictionary<long, int> eventRaiseCountDictionary = new SortedDictionary<long, int>();

        private static List<long> blacklistedUserIds = new List<long>();

        private static Timer eventCountInitTimer;

        private static Timer blacklistInitTimer;

        private static void InitInvisibleSomething()
        {
            eventCountInitTimer = new Timer(
                _ => { lock (listLock) { eventRaiseCountDictionary.Clear(); } },
                null,
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(3));
            blacklistInitTimer = new Timer(
                _ => { lock (listLock) { blacklistedUserIds.Clear(); } },
                null,
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(30));
        }

        private static bool IsBlacklisted(TwitterUser user)
        {
            lock (listLock)
            {
                if (eventRaiseCountDictionary.ContainsKey(user.NumericId))
                    eventRaiseCountDictionary[user.NumericId]++;
                else
                    eventRaiseCountDictionary.Add(user.NumericId, 1);
                if (blacklistedUserIds.Contains(user.NumericId))
                    return true;
                if (eventRaiseCountDictionary[user.NumericId] >= BLACKLIST_THRESHOLD)
                {
                    blacklistedUserIds.Add(user.NumericId);
                    Register(new EventDescription(EventKind.Suppressed, new UserViewModel(user), null), true);
                    return true;
                }
                else
                {
                    // OK
                    return false;
                }
            }
        }
        #endregion
    }

    public class EventDescription
    {
        public EventKind Kind { get; private set; }

        public UserViewModel SourceUser { get; private set; }

        public UserViewModel TargetUser { get; private set; }

        public TweetViewModel TargetTweet { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public EventDescription(EventKind kind, UserViewModel source, UserViewModel target, TweetViewModel tweet = null)
        {
            this.CreatedAt = DateTime.Now;
            this.Kind = kind;
            this.SourceUser = source;
            this.TargetUser = target;
            this.TargetTweet = tweet;
        }
    }

    public enum EventKind
    {
        Retweet,
        Favorite,
        Unfavorite,
        Follow,
        Unfollow,
        Mention,
        DirectMessage,
        Undefined,
        Suppressed,
    }

    public class EventDescriptionEventArgs : EventArgs
    {
        public EventDescription EventDescription { get; private set; }
        public EventDescriptionEventArgs(EventDescription desc)
        {
            this.EventDescription = desc;
        }
    }
}
