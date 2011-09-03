using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Dulcet.Twitter;
using Inscribe.Configuration.Accounts;
using Inscribe.Data;
using Inscribe.Storage;
using Inscribe.ViewModels.PartBlocks.MainBlock;
using Livet;

namespace Inscribe.Authentication
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

        #region ConnectionStateChangedイベント

        public event EventHandler<EventArgs> ConnectionStateChanged;
        private Notificator<EventArgs> _ConnectionStateChangedEvent;
        public Notificator<EventArgs> ConnectionStateChangedEvent
        {
            get
            {
                if (_ConnectionStateChangedEvent == null)
                    _ConnectionStateChangedEvent = new Notificator<EventArgs>();
                return _ConnectionStateChangedEvent;
            }
            set { _ConnectionStateChangedEvent = value; }
        }

        protected void OnConnectionStateChanged(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref ConnectionStateChanged, null, null);
            if (threadSafeHandler != null) threadSafeHandler(this, e);
            ConnectionStateChangedEvent.Raise(e);
        }

        #endregion


        private ConnectionState _connectionState = ConnectionState.Disconnected;
        public ConnectionState ConnectionState
        {
            get { return this._connectionState; }
            set
            {
                if (this._connectionState == value) return;
                this._connectionState = value;
                OnConnectionStateChanged(EventArgs.Empty);
            }
        }

        #region Additional information

        private SafeLinkedList<long> following = new SafeLinkedList<long>();

        private SafeLinkedList<long> followers = new SafeLinkedList<long>();

        private SafeLinkedList<long> blockings = new SafeLinkedList<long>();

        private SafeLinkedList<TwitterList> followingLists = new SafeLinkedList<TwitterList>();

        #endregion

        #region Following

        public void RegisterFollowing(long user)
        {
            if (!this.following.Contains(user))
                this.following.AddLast(user);
        }

        public void RemoveFollowing(long user)
        {
            this.following.Remove(user);
        }

        [XmlIgnore()]
        public IEnumerable<long> Followings
        {
            get { return this.following; }
        }

        /// <summary>
        /// 指定したユーザーをフォローしているか　
        /// </summary>
        public bool IsFollowing(long id)
        {
            return this.following.Contains(id);
        }

        #endregion

        #region Followers

        public void RegisterFollower(long user)
        {
            if (!this.followers.Contains(user))
                this.followers.AddLast(user);
        }

        public void RemoveFollower(long user)
        {
            this.followers.Remove(user);
        }

        [XmlIgnore()]
        public IEnumerable<long> Followers
        {
            get { return this.followers; }
        }

        /// <summary>
        /// 指定したユーザーからフォローされているか
        /// </summary>
        public bool IsFollowedBy(long id)
        {
            return this.followers.Contains(id);
        }

        #endregion

        #region Blockings

        public void RegisterBlocking(long user)
        {
            if (!this.blockings.Contains(user))
                this.blockings.AddLast(user);
        }

        public void RemoveBlocking(long user)
        {
            this.blockings.Remove(user);
        }

        [XmlIgnore()]
        public IEnumerable<long> Blockings
        {
            get { return this.blockings; }
        }

        public bool IsBlocking(long id)
        {
            return this.blockings.Contains(id);
        }

        #endregion

        #region Lists

        public void RegisterFollowingList(TwitterList list)
        {
            if (this.followingLists.FirstOrDefault(l => l.User.ScreenName == list.User.ScreenName && l.Name == list.Name) != null)
                return;
            this.followingLists.AddLast(list);
            ListStorage.Register(list);
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

        #region Reference helper

        public Uri ProfileImage
        {
            get
            {
                try
                {
                    return this.UserViewModel.TwitterUser.ProfileImage;
                }
                catch { return null; }
            }
        }

        #endregion

    }
}
