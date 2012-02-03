using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dulcet.Twitter;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.Util;
using Inscribe.Storage;
using Livet;
using Livet.Commands;
using Inscribe.Text;
using Inscribe.Plugin;
using System.Threading.Tasks;

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

        /// <summary>
        /// Retweetを考慮したすべての本文を取得します。
        /// </summary>
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

        /// <summary>
        /// 何も考慮せず本文を返します。<para />
        /// Retweetされたツイートの場合、RTが付きます。また、途切れる可能性があります。
        /// </summary>
        public string Text
        {
            get { return this.Status.Text; }
        }

        /// <summary>
        /// このツイートのURL(Permalink)を取得します。
        /// </summary>
        public string Permalink
        {
            get
            {
                return "http://twitter.com/" + this.Status.User.ScreenName + "/status/" + this.Status.Id.ToString();
            }
        }

        public string OriginalPermalink
        {
            get
            {
                TwitterStatus stat;
                return (stat = this.Status as TwitterStatus) != null &&
                    stat.RetweetedOriginal != null ?
                    "http://twitter.com/" + stat.User.ScreenName + "/status/" + stat.Id.ToString() :
                    "http://twitter.com/" + this.Status.User.ScreenName + "/status/" + this.Status.Id.ToString();
            }
        }

        #endregion

        #region Inline photos property

        private bool _isPhotoResolving = false;

        /// <summary>
        /// 画像表示を解決中
        /// </summary>
        public bool IsPhotoResolving
        {
            get { return _isPhotoResolving; }
            set
            {
                _isPhotoResolving = value;
                RaisePropertyChanged(() => IsPhotoResolving);
            }
        }

        private IEnumerable<PhotoThumbnailViewModel> _photoThumbUrls = null;
        public IEnumerable<PhotoThumbnailViewModel> PhotoThumbnails
        {
            get
            {
                if (Setting.Instance.TweetExperienceProperty.ShowImageInlineThumbnail)
                {
                    if (_photoThumbUrls == null && !IsPhotoResolving)
                    {
                        Task.Factory.StartNew(() => ResolvePhotos());
                    }
                    return _photoThumbUrls ?? new PhotoThumbnailViewModel[0];
                }
                else
                {
                    return new PhotoThumbnailViewModel[0];
                }
            }
        }

        private void ResolvePhotos()
        {
            var tokens = Tokenizer.Tokenize(this.TweetText);
            var uris = tokens
                .Where(t => t.Kind == TokenKind.Url).ToArray();

            if (uris.Length > 0)
            {
                IsPhotoResolving = true;
                _photoThumbUrls = uris
                    .Select(t => UploaderManager.TryResolve(ShortenManager.Extract(t.Text)))
                    .Where(u => u != null)
                    .Select(s => new PhotoThumbnailViewModel(new Uri(s)))
                    .ToArray();
                IsPhotoResolving = false;
                RaisePropertyChanged(() => PhotoThumbnails);
            }
            else
            {
                _photoThumbUrls = new PhotoThumbnailViewModel[0];
                IsPhotoResolving = false;
            }
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
            // RaisePropertyChanged(() => RetweetedUsers);
            RaisePropertyChanged(() => RetweetedUsersCount);
            RaisePropertyChanged(() => IsRetweetExists);
            RaisePropertyChanged(() => IsRetweeted);
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
            // RaisePropertyChanged(() => RetweetedUsers);
            RaisePropertyChanged(() => RetweetedUsersCount);
            RaisePropertyChanged(() => IsRetweetExists);
            RaisePropertyChanged(() => IsRetweeted);
            return true;
        }

        public ConcurrentObservable<UserViewModel> RetweetedUsers
        {
            get { return this._retweeteds; }
        }

        public int RetweetedUsersCount
        {
            get { return this.RetweetedUsers.Count; }
        }

        public bool IsRetweetExists
        {
            get { return this._retweeteds.Count > 0; }
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
            // RaisePropertyChanged(() => FavoredUsers);
            RaisePropertyChanged(() => FavoredUsersCount);
            RaisePropertyChanged(() => IsFavorExists);
            RaisePropertyChanged(() => IsFavored);
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
            // RaisePropertyChanged(() => FavoredUsers);
            RaisePropertyChanged(() => FavoredUsersCount);
            RaisePropertyChanged(() => IsFavorExists);
            RaisePropertyChanged(() => IsFavored);
            return true;
        }

        public ConcurrentObservable<UserViewModel> FavoredUsers
        {
            get { return this._favoreds; }
        }

        public int FavoredUsersCount
        {
            get { return this._favoreds.Count; }
        }

        public bool IsFavorExists
        {
            get { return this._favoreds.Count > 0; }
        }

        #endregion

        #region Reply Chains Control

        /// <summary>
        /// このツイートに返信しているツイートのID
        /// </summary>
        private SafeList<long> inReplyFroms = new SafeList<long>();

        /// <summary>
        /// このツイートに返信を行っていることを登録します。
        /// </summary>
        /// <param name="tweetId">返信しているツイートのID</param>
        public void RegisterInReplyToThis(long tweetId)
        {
            this.inReplyFroms.Add(tweetId);
            TweetStorage.NotifyTweetStateChanged(this);
            RaisePropertyChanged(() => IsMentioned);
        }

        public void RemoveInReplyToThis(long tweetId)
        {
            this.inReplyFroms.Remove(tweetId);
            TweetStorage.NotifyTweetStateChanged(this);
            RaisePropertyChanged(() => IsMentioned);
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
            RaisePropertyChanged(() => CanFavorite);
            RaisePropertyChanged(() => IsExpandedView);
            RaisePropertyChanged(() => IsFullLineView);
            RaisePropertyChanged(() => TextWrapping);
            RaisePropertyChanged(() => TextTrimming);
            RaisePropertyChanged(() => IsNameBackColoring);
            RaisePropertyChanged(() => IsBottomBarColoring);
            RaisePropertyChanged(() => BottomBarHeight);
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
            get { return Setting.Instance.TweetExperienceProperty.UseP3StyleIcon; }
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
                return status != null && 
                    (status.InReplyToStatusId != 0 ||
                    (status.RetweetedOriginal != null && status.RetweetedOriginal.InReplyToStatusId != 0));
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

        public bool IsRetweeted
        {
            get
            {
                return TwitterHelper.IsRetweetedThis(this);
            }
        }

        public bool IsMentioned
        {
            get
            {
                if (this.inReplyFroms.Count == 0)
                    return false;
                return inReplyFroms.Select(i => TweetStorage.Get(i))
                    .Where(vm => vm != null)
                    .Select(vm => TwitterHelper.IsMyTweet(vm))
                    .Any(b => b);
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
                else if (status != null && status.RetweetedOriginal != null && status.RetweetedOriginal.InReplyToStatusId != 0)
                {
                    var tweet = TweetStorage.Get(status.RetweetedOriginal.InReplyToStatusId);
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

        public bool IsNameBackColoring
        {
            get { return Setting.Instance.ColoringProperty.TweetColorMode == Configuration.Settings.TweetColoringMode.NameBackground; }
        }

        public bool IsBottomBarColoring
        {
            get
            {
                return Setting.Instance.ColoringProperty.TweetColorMode == Configuration.Settings.TweetColoringMode.BottomBar ||
                    Setting.Instance.ColoringProperty.TweetColorMode == Configuration.Settings.TweetColoringMode.BottomBarGradient;
            }
        }

        public double BottomBarHeight
        {
            get
            {
                return
                    Setting.Instance.ColoringProperty.TweetColorMode == Configuration.Settings.TweetColoringMode.BottomBar ?
                    2 : 5;
            }
        }

        public bool IsExpandedView
        {
            get { return Setting.Instance.TweetExperienceProperty.TweetViewMode == Configuration.Settings.TweetViewingMode.Expanded; }
        }

        public bool IsFullLineView
        {
            get { return Setting.Instance.TweetExperienceProperty.TweetViewMode == Configuration.Settings.TweetViewingMode.FullLine; }
        }

        public TextWrapping TextWrapping
        {
            get { return IsFullLineView ? TextWrapping.Wrap : TextWrapping.NoWrap; }
        }

        public TextTrimming TextTrimming
        {
            get { return IsFullLineView ? TextTrimming.None : TextTrimming.CharacterEllipsis; }
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

        public DateTime OriginalCreatedAt
        {
            get
            {
                TwitterStatus stat;
                return (stat = this.Status as TwitterStatus) != null &&
                    stat.RetweetedOriginal != null ?
                    stat.RetweetedOriginal.CreatedAt :
                    this.CreatedAt;
            }
        }

        public bool CanFavorite
        {
            get
            {
                return this.Status is TwitterStatus &&
                    (!this.IsMyTweet &&
                    (((TwitterStatus)this.Status).RetweetedOriginal == null ||
                        AccountStorage.Get(((TwitterStatus)this.Status).RetweetedOriginal.User.ScreenName) == null)
                     || Setting.Instance.TweetExperienceProperty.CanFavoriteMyTweet);
            }
        }

        public bool ShowTooltip
        {
            get { return Setting.Instance.TweetExperienceProperty.ShowTweetTooltip; }
        }

        public bool IsQuickFavAndRetweetEnabled
        {
            get { return Setting.Instance.TweetExperienceProperty.QuickFavAndRetweet; }
        }

        #endregion

        #region Commands

        #region CopySTOTCommand
        ViewModelCommand _CopySTOTCommand;

        public ViewModelCommand CopySTOTCommand
        {
            get
            {
                if (_CopySTOTCommand == null)
                    _CopySTOTCommand = new ViewModelCommand(CopySTOT);
                return _CopySTOTCommand;
            }
        }

        private void CopySTOT()
        {
            CopyClipboard(TwitterHelper.GetSuggestedUser(this).ScreenName + ":" +
                this.TweetText + " [" + this.Permalink + "]");

        }
        #endregion

        #region CopyWebUrlCommand
        ViewModelCommand _CopyWebUrlCommand;

        public ViewModelCommand CopyWebUrlCommand
        {
            get
            {
                if (_CopyWebUrlCommand == null)
                    _CopyWebUrlCommand = new ViewModelCommand(CopyWebUrl);
                return _CopyWebUrlCommand;
            }
        }

        private void CopyWebUrl()
        {
            CopyClipboard(this.Permalink);
        }
        #endregion

        #region CopyScreenNameCommand
        ViewModelCommand _CopyScreenNameCommand;

        public ViewModelCommand CopyScreenNameCommand
        {
            get
            {
                if (_CopyScreenNameCommand == null)
                    _CopyScreenNameCommand = new ViewModelCommand(CopyScreenName);
                return _CopyScreenNameCommand;
            }
        }

        private void CopyScreenName()
        {
            CopyClipboard(TwitterHelper.GetSuggestedUser(this).ScreenName);
        }
        #endregion

        private void CopyClipboard(string text)
        {
            try
            {
                Clipboard.SetText(text);
                NotifyStorage.Notify("コピーしました: " + text);
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.InternalError,
                    "コピーに失敗しました");
            }
        }

        #region ShowTweetCommand

        ViewModelCommand _ShowTweetCommand;

        public ViewModelCommand ShowTweetCommand
        {
            get
            {
                if (_ShowTweetCommand == null)
                    _ShowTweetCommand = new ViewModelCommand(ShowTweet);
                return _ShowTweetCommand;
            }
        }

        private void ShowTweet()
        {
            Browser.Start(this.Permalink);
        }


        #region ShowOriginalTweetCommand
        ViewModelCommand _ShowOriginalTweetCommand;

        public ViewModelCommand ShowOriginalTweetCommand
        {
            get
            {
                if (_ShowOriginalTweetCommand == null)
                    _ShowOriginalTweetCommand = new ViewModelCommand(ShowOriginalTweet);
                return _ShowOriginalTweetCommand;
            }
        }

        private void ShowOriginalTweet()
        {
            Browser.Start(this.OriginalPermalink);
        }
        #endregion
      

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
