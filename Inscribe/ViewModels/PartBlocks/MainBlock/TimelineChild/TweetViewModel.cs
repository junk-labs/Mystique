using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dulcet.Twitter;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.Data;
using Inscribe.Storage;
using Livet;
using Livet.Commands;

namespace Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild
{
    /// <summary>
    /// ツイートを保持するViewModel
    /// </summary>
    public class TweetViewModel : ViewModel
    {
        public TwitterStatusBase Status { get; private set; }

        public readonly long bindingId;

        public TweetViewModel(TwitterStatusBase status)
        {
            if (status == null)
                throw new ArgumentNullException("status");
            this.bindingId = status.Id;
            this.Status = status;
        }

        public TweetViewModel(long id)
        {
            this.bindingId = id;
        }


        /// <summary>
        /// まだステータス情報が関連付けられていない場合に、ステータス情報を関連付けます。
        /// </summary>
        public void SetStatus(TwitterStatusBase status)
        {
            if (this.Status != null) return;
            if (status.Id != bindingId)
                throw new ArgumentException("ステータスIDが一致しません。");
            this.Status = status;
        }

        /// <summary>
        /// このステータスがステータス情報を保持しているか
        /// </summary>
        public bool IsStatusInfoContains
        {
            get { return this.Status != null; }
        }

        public string TweetText
        {
            get
            {
                var t = this.Status as TwitterStatus;
                if (t != null && t.RetweetedOriginal != null)
                    return t.RetweetedOriginal.Text;
                else
                    return this.Status.Text;
            }
        }

        #region Twitter Status Property

        public Uri ProfileImage
        {
            get { return this.Status.User.ProfileImage; }
        }

        public Uri DirectMessageReceipientImage
        {
            get
            {
                var dm = this.Status as TwitterDirectMessage;
                if (dm == null)
                    return null;
                else
                    return dm.Recipient.ProfileImage;
            }
        }

        public string Text
        {
            get { return this.Status.Text; }
        }

        #endregion

        #region Retweeteds Control

        private ConcurrentObservable<UserViewModel> _retweeteds = new ConcurrentObservable<UserViewModel>();

        public bool RegisterRetweeted(UserViewModel user)
        {
            lock (_retweeteds)
            {
                if (user == null || this._retweeteds.Select(s => s.TwitterUser.ScreenName)
                    .FirstOrDefault(s => s == user.TwitterUser.ScreenName) != null)
                    return false;
                this._retweeteds.Add(user);
            }
            TweetStorage.NotifyTweetStateChanged(this);
            RaisePropertyChanged(() => RetweetedUsers);
            return true;
        }

        public bool RemoveRetweeted(UserViewModel user)
        {
            lock (_retweeteds)
            {
                if (user == null || this._retweeteds.Select(s => s.TwitterUser.ScreenName).FirstOrDefault(s => s == user.TwitterUser.ScreenName) == null)
                    return false;
                this._retweeteds.Remove(user);
            }
            TweetStorage.NotifyTweetStateChanged(this);
            RaisePropertyChanged(() => RetweetedUsers);
            return true;
        }

        public ConcurrentObservable<UserViewModel> RetweetedUsers
        {
            get { return this._retweeteds; }
        }

        #endregion

        #region Favored Control

        private ConcurrentObservable<UserViewModel> _favoreds = new ConcurrentObservable<UserViewModel>();

        public bool RegisterFavored(UserViewModel user)
        {
            lock (_favoreds)
            {
                if (user == null || this._favoreds.Select(s => s.TwitterUser.ScreenName)
                    .FirstOrDefault(s => s == user.TwitterUser.ScreenName) != null)
                    return false;
                this._favoreds.Add(user);
            }
            TweetStorage.NotifyTweetStateChanged(this);
            RaisePropertyChanged(() => FavoredUsers);
            return true;
        }

        public bool RemoveFavored(UserViewModel user)
        {
            lock (_favoreds)
            {
                if (user == null || this._favoreds.Select(s => s.TwitterUser.ScreenName).FirstOrDefault(s => s == user.TwitterUser.ScreenName) == null)
                    return false;
                this._favoreds.Remove(user);
            }
            TweetStorage.NotifyTweetStateChanged(this);
            RaisePropertyChanged(() => FavoredUsers);
            return true;
        }

        public ConcurrentObservable<UserViewModel> FavoredUsers
        {
            get { return this._favoreds; }
        }

        #endregion

        #region Reply Chains Control

        /// <summary>
        /// このツイートに返信しているツイートのID
        /// </summary>
        private ConcurrentBag<long> inReplyFroms = new ConcurrentBag<long>();

        /// <summary>
        /// このツイートに返信を行っていることを登録します。
        /// </summary>
        /// <param name="tweetId">返信しているツイートのID</param>
        public void RegisterInReplyToThis(long tweetId)
        {
            this.inReplyFroms.Add(tweetId);
            TweetStorage.NotifyTweetStateChanged(this);
        }

        /// <summary>
        /// このツイートに返信しているツイートID
        /// </summary>
        public IEnumerable<long> InReplyFroms
        {
            get { return this.inReplyFroms; }
        }

        #endregion

        #region Explicit Controlling Methods

        public void SettingValueChanged()
        {
            RaisePropertyChanged(() => Status);
            RaisePropertyChanged(() => NameAreaWidth);
        }

        public void RefreshInReplyToInfo()
        {
            RaisePropertyChanged(() => ReplyText);
        }

        #endregion

        #region Setting dependent property

        public double NameAreaWidth
        {
            get { return (double)Setting.Instance.TweetExperienceProperty.NameAreaWidth; }
        }

        public bool IsP3StyleIcon
        {
            get { return Setting.Instance.TweetExperienceProperty.P3StyleIcon; }
        }

        #endregion

        #region Binding Helper Property

        public bool IsProtected
        {
            get { return TwitterHelper.GetSuggestedUser(this).IsProtected; }
        }

        public bool IsVerified
        {
            get { return TwitterHelper.GetSuggestedUser(this).IsVerified; }
        }

        public bool IsStatus
        {
            get { return this.Status is TwitterStatus; }
        }

        public bool IsDirectMessage
        {
            get
            {
                return this.Status is TwitterDirectMessage;
            }
        }

        public bool IsMention
        {
            get
            {
                var status = this.Status as TwitterStatus;
                return status != null && status.InReplyToStatusId != 0;
            }
        }

        public bool IsMentionToMe
        {
            get
            {
                return TwitterHelper.IsInReplyToMe(this);
            }
        }

        public bool IsPublishedByRetweet
        {
            get
            {
                return TwitterHelper.IsPublishedByRetweet(this);
            }
        }

        public bool IsFavored
        {
            get
            {
                return TwitterHelper.IsFavoredThis(this);
            }
        }

        public string ReplyText
        {
            get
            {
                var status = this.Status as TwitterStatus;
                if (status != null && status.InReplyToStatusId != 0)
                {
                    var tweet = TweetStorage.Get(status.InReplyToStatusId);
                    if (tweet == null || !tweet.IsStatusInfoContains)
                        return "受信していません";
                    else
                        return "@" + tweet.Status.User.ScreenName + ": " + tweet.Status.Text;
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        public bool ShowRetweetButton
        {
            get
            {
                return !this.IsProtected;
            }
        }

        public bool ShowUnofficialRetweetButton
        {
            get
            {
                return Setting.Instance.TweetExperienceProperty.ShowUnofficialRetweetButton && !this.IsProtected;
            }
        }

        public bool ShowQuoteButton
        {
            get
            {
                return Setting.Instance.TweetExperienceProperty.ShowQuoteButton;
            }
        }

        public bool ShowDeleteButton
        {
            get { return AccountStorage.Contains(this.Status.User.ScreenName); }
        }

        public bool IsMyTweet
        {
            get
            {
                return AccountStorage.Get(this.Status.User.ScreenName) != null;
            }
        }

        public DateTime CreatedAt
        {
            get
            {
                if (this.Status == null)
                    return DateTime.MinValue;
                else
                    return this.Status.CreatedAt;
            }
        }

        #endregion

        #region Commands

        #region ShowTweetCommand

        DelegateCommand _ShowTweetCommand;

        public DelegateCommand ShowTweetCommand
        {
            get
            {
                if (_ShowTweetCommand == null)
                    _ShowTweetCommand = new DelegateCommand(ShowTweet);
                return _ShowTweetCommand;
            }
        }

        private void ShowTweet()
        {
            Browser.Start("http://twitter.com/" + this.Status.User.ScreenName + "/status/" + this.Status.Id.ToString());
        }

        #endregion

        #endregion

        public override bool Equals(object obj)
        {
            var tdtv = obj as TweetViewModel;
            if (tdtv != null)
                return this.bindingId == tdtv.bindingId;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return (int)this.bindingId;
        }
    }

}
