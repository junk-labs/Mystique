using System;
using System.Collections.Generic;
using Dulcet.Twitter;
using Livet;
using Inscribe.Data;

namespace Inscribe.ViewModels
{
    /// <summary>
    /// ツイートを保持するViewModel
    /// </summary>
    public class TweetViewModel : ViewModel
    {
        public TwitterStatusBase Status { get; private set; }

        public TweetViewModel(TwitterStatusBase status = null)
        {
            this.Status = status;
        }

        /// <summary>
        /// まだステータス情報が関連付けられていない場合に、ステータス情報を関連付けます。
        /// </summary>
        public void SetStatus(TwitterStatusBase status)
        {
            if (this.Status != null)
                throw new InvalidOperationException("すでにステータスが実体化されています。");
            this.Status = status;
        }

        /// <summary>
        /// このステータスがステータス情報を保持しているか
        /// </summary>
        public bool IsStatusInfoContains
        {
            get { return this.Status != null; }
        }

        #region Retweeteds Control

        private SafeObservable<UserViewModel> _retweeteds = new SafeObservable<UserViewModel>();

        public void RegisterRetweeted(UserViewModel user)
        {
            if (user == null)
                return;
            this._retweeteds.Add(user);
        }

        public void RemoveRetweeted(UserViewModel user)
        {
            if (user == null)
                return;
            this._retweeteds.Remove(user);
        }

        public IEnumerable<UserViewModel> RetweetedUsers
        {
            get { return this._retweeteds; }
        }

        #endregion

        #region Favored Control

        private SafeObservable<UserViewModel> _favoreds = new SafeObservable<UserViewModel>();

        public void RegisterFavored(UserViewModel user)
        {
            if (user == null)
                return;
            this._favoreds.Add(user);
        }

        public void RemoveFavored(UserViewModel user)
        {
            if (user == null)
                return;
            this._favoreds.Remove(user);
        }

        public IEnumerable<UserViewModel> FavoredUsers
        {
            get { return this._favoreds; }
        }

        #endregion

        #region Reply Chains Control

        /// <summary>
        /// このツイートに返信しているツイートのID
        /// </summary>
        private SafeLinkedList<long> inReplyFroms = new SafeLinkedList<long>();

        /// <summary>
        /// このツイートに返信を行っていることを登録します。
        /// </summary>
        /// <param name="tweetId">返信しているツイートのID</param>
        public void RegisterInReplyToThis(long tweetId)
        {
            this.inReplyFroms.AddLast(tweetId);
        }

        /// <summary>
        /// このツイートに返信しているツイートID
        /// </summary>
        public IEnumerable<long> InReplyFroms
        {
            get { return this.inReplyFroms; }
        }

        #endregion
    }
}
