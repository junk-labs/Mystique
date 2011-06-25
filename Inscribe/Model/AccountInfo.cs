using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Dulcet.Twitter;
using Inscribe.Configuration.Elements;
using Inscribe.Data;
using Inscribe.ViewModels;
using System.Collections.Concurrent;
using Livet;
using System.Threading;

namespace Inscribe.Model
{
    public enum ConnectionState
    {
        WaitNetwork,
        WaitTwitter,
        TryConnection,
        Connected,
        Disconnected,
    }

    /// <summary>
    /// アカウント情報を保持するクラスです。
    /// </summary>
    public class AccountInfo : CredentialCore
    {
        /// <summary>
        /// クレデンシャル情報からアカウント情報を生成します。
        /// </summary>
        /// <param name="original">オリジナル クレデンシャル</param>
        /// <returns>アカウント情報</returns>
        public static AccountInfo FromCredential(CredentialCore original)
        {
            return new AccountInfo()
            {
                OverridedConsumerKey = original.OverridedConsumerKey,
                OverridedConsumerSecret = original.OverridedConsumerSecret,
                RateLimitMax = original.RateLimitMax,
                RateLimitRemaining = original.RateLimitRemaining,
                RateLimitReset = original.RateLimitReset,
                ScreenName = original.ScreenName,
                Secret = original.Secret,
                Token = original.Token,
            };
        }

        public AccountInfo()
        {
            this.AccoutProperty = new AccountProperty();
        }

        /// <summary>
        /// このアカウント情報のクレデンシャルを書き換えます。
        /// </summary>
        /// <param name="overwrite">このアカウント情報に上書きするクレデンシャル</param>
        public void RewriteCredential(CredentialCore overwrite)
        {
            this.OverridedConsumerKey = overwrite.OverridedConsumerKey;
            this.OverridedConsumerSecret = overwrite.OverridedConsumerSecret;
            this.RateLimitMax = overwrite.RateLimitMax;
            this.RateLimitRemaining = overwrite.RateLimitRemaining;
            this.RateLimitReset = overwrite.RateLimitReset;
            this.ScreenName = overwrite.ScreenName;
            this.Secret = overwrite.Secret;
            this.Token = overwrite.Token;
        }

        [XmlIgnore()]
        public UserViewModel UserViewModel
        {
            get { return Storage.UserStorage.Get(this.ScreenName); }
        }

        #region UserStreamsConnectionChangedイベント

        public event EventHandler<EventArgs> UserStreamsConnectionChanged;
        private Notificator<EventArgs> _UserStreamsConnectionChangedEvent;
        public Notificator<EventArgs> UserStreamsConnectionChangedEvent
        {
            get
            {
                if (_UserStreamsConnectionChangedEvent == null) _UserStreamsConnectionChangedEvent = new Notificator<EventArgs>();
                return _UserStreamsConnectionChangedEvent;
            }
            set { _UserStreamsConnectionChangedEvent = value; }
        }

        protected void OnUserStreamsConnectionChanged(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref UserStreamsConnectionChanged, null, null);
            if (threadSafeHandler != null) threadSafeHandler(this, e);
            UserStreamsConnectionChangedEvent.Raise(e);
        }

        #endregion


        private ConnectionState _userStreamsConnectionState = ConnectionState.Disconnected;
        public ConnectionState UserStreamsConnectionState
        {
            get { return this._userStreamsConnectionState; }
            set
            {
                if (this._userStreamsConnectionState == value) return;
                this._userStreamsConnectionState = value;
                OnUserStreamsConnectionChanged(EventArgs.Empty);
            }
        }

        #region Additional information

        private SafeLinkedList<UserViewModel> following = new SafeLinkedList<UserViewModel>();

        private SafeLinkedList<UserViewModel> followers = new SafeLinkedList<UserViewModel>();

        private SafeLinkedList<UserViewModel> blockings = new SafeLinkedList<UserViewModel>();

        private SafeLinkedList<TwitterList> followingLists = new SafeLinkedList<TwitterList>();

        #endregion

        #region Following

        public void RegisterFollowing(UserViewModel user)
        {
            if (!this.following.Contains(user))
                this.following.AddLast(user);
        }

        public void RemoveFollowing(UserViewModel user)
        {
            this.following.Remove(user);
        }

        [XmlIgnore()]
        public IEnumerable<UserViewModel> Followings
        {
            get { return this.following; }
        }

        /// <summary>
        /// 指定したユーザーをフォローしているか　
        /// </summary>
        public bool IsFollowing(string screenName)
        {
            return this.following
                .FirstOrDefault(v => v.TwitterUser.ScreenName == screenName) != null;
        }

        #endregion

        #region Followers

        public void RegisterFollower(UserViewModel user)
        {
            if (!this.followers.Contains(user))
                this.followers.AddLast(user);
        }

        public void RemoveFollower(UserViewModel user)
        {
            this.followers.Remove(user);
        }

        [XmlIgnore()]
        public IEnumerable<UserViewModel> Followers
        {
            get { return this.followers; }
        }

        /// <summary>
        /// 指定したユーザーからフォローされているか
        /// </summary>
        public bool IsFollowedBy(string screenName)
        {
            return this.followers
                .FirstOrDefault(v => v.TwitterUser.ScreenName == screenName) != null;
        }

        #endregion

        #region Blockings

        public void RegisterBlocking(UserViewModel user)
        {
            if (!this.blockings.Contains(user))
                this.blockings.AddLast(user);
        }

        public void RemoveBlocking(UserViewModel user)
        {
            this.blockings.Remove(user);
        }

        [XmlIgnore()]
        public IEnumerable<UserViewModel> Blockings
        {
            get { return this.blockings; }
        }

        public bool IsBlocking(string screenName)
        {
            return this.blockings
                .FirstOrDefault(v => v.TwitterUser.ScreenName == screenName) != null;
        }

        #endregion

        #region Lists

        public void RegisterFollowingList(TwitterList list)
        {
            this.followingLists.AddLast(list);
        }

        public void RemoveFollowingList(TwitterList list)
        {
            this.followingLists.Remove(list);
        }

        [XmlIgnore()]
        public IEnumerable<TwitterList> FollowingLists
        {
            get { return this.followingLists; }
        }

        /// <summary>
        /// 指定したリストを購読しているか
        /// </summary>
        public bool IsFollowingList(string userScreenName, string listName)
        {
            return this.followingLists
                .FirstOrDefault(l => l.User.ScreenName == userScreenName && l.CompareListName(listName)) != null;
        }

        #endregion

        public AccountProperty AccoutProperty { get; set; }

    }
}
