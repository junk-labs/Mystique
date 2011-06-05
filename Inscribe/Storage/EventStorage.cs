using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet.Command;
using Inscribe.Data;
using Inscribe.ViewModels;
using Livet;
using System.Threading;

namespace Inscribe.Storage
{
    public static class EventStorage
    {
        private static SafeLinkedList<EventDescription> events = new SafeLinkedList<EventDescription>();

        public static IEnumerable<EventDescription> Events
        {
            get { return events; }
        }

        public static void Remove(EventDescription evd)
        {
            events.Remove(evd);
        }

        private static void Register(EventDescription description)
        {
            events.AddLast(description);
            OnEventRegistered(EventArgs.Empty);
        }

        #region EventRegisteredイベント

        public static event EventHandler<EventArgs> EventRegistered;
        private static Notificator<EventArgs> _EventRegisteredEvent;
        public static Notificator<EventArgs> EventRegisteredEvent
        {
            get
            {
                if (_EventRegisteredEvent == null) _EventRegisteredEvent = new Notificator<EventArgs>();
                return _EventRegisteredEvent;
            }
            set { _EventRegisteredEvent = value; }
        }

        private static void OnEventRegistered(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref EventRegistered, null, null);
            if (threadSafeHandler != null) threadSafeHandler(null, e);
            EventRegisteredEvent.Raise(e);
        }

        #endregion

        public static void OnRetweeted(TweetViewModel tweet, UserViewModel retweeter)
        {
            Register(new EventDescription(EventKind.Retweet, retweeter,
                    UserStorage.Get(tweet.Status.User), tweet));
        }

        public static void OnFavored(TweetViewModel tweet, UserViewModel favorer)
        {
            Register(new EventDescription(EventKind.Favorite,
                favorer, UserStorage.Get(tweet.Status.User), tweet));
        }

        public static void OnUnfavored(TweetViewModel tweet, UserViewModel favorer)
        {
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
        Unfollow
    }
}
