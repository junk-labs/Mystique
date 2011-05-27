using System;
using System.Collections.Generic;
using Dulcet.Twitter;
using Livet;
using Inscribe.Data;
using Inscribe.Configuration.Tabs;
using System.Collections.Concurrent;
using System.Windows;
using Inscribe.Configuration;
using System.Windows.Media;
using System.Linq;
using Inscribe.Configuration.Settings;
using Inscribe.Common;
using System.Threading.Tasks;
using Inscribe.Storage;
using Livet.Command;

namespace Inscribe.ViewModels
{
    /// <summary>
    /// ツイートを保持するViewModel
    /// </summary>
    public class TweetViewModel : ViewModel
    {
        public TwitterStatusBase Status { get; private set; }

        public TabViewModel ParentTabViewModel { get; set; }

        public TweetViewModel(TwitterStatusBase status = null, DateTime? receiveTime = null)
        {
            this.Status = status;
            this._registeredDateTime = receiveTime;
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

        private ConcurrentObservable<UserViewModel> _retweeteds = new ConcurrentObservable<UserViewModel>();

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

        private ConcurrentObservable<UserViewModel> _favoreds = new ConcurrentObservable<UserViewModel>();

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
        private ConcurrentBag<long> inReplyFroms = new ConcurrentBag<long>();

        /// <summary>
        /// このツイートに返信を行っていることを登録します。
        /// </summary>
        /// <param name="tweetId">返信しているツイートのID</param>
        public void RegisterInReplyToThis(long tweetId)
        {
            this.inReplyFroms.Add(tweetId);
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

        /// <summary>
        /// 色の変更があったことを通知します。
        /// </summary>
        /// <param name="isRefreshBrightColor">ブライトカラーを更新するか。ステータスの選択変更に起因する通知の場合はここをfalseに設定します。</param>
        public void PendingColorChanged(bool isRefreshBrightColor = false)
        {
            this.isColorChanged = true;
            if (isRefreshBrightColor)
                this.lightningColorChanged = true;
            RaisePropertyChanged(() => BackBrush);
        }

        public void SettingValueChanged()
        {
            PendingColorChanged(true);
            RaisePropertyChanged(() => Status);
            RaisePropertyChanged(() => NameAreaWidth);
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

        private double _tooltipWidth = 0;
        public double TooltipWidth
        {
            get { return _tooltipWidth; }
            set
            {
                _tooltipWidth = value;
                RaisePropertyChanged(() => TooltipWidth);
            }
        }

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
                var status = Status as TwitterStatus;
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

        public bool ShowQuoteTweetButton
        {
            get
            {
                return Setting.Instance.TweetExperienceProperty.ShowQuoteTweetButton;
            }
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

        private DateTime? _registeredDateTime = null;
        public DateTime OrderByKeyDateTime
        {
            get
            {
                return _registeredDateTime ?? CreatedAt;
            }
        }

        #endregion

        #region Coloring Property

        private bool lightningColorChanged = true;
        private bool isColorChanged = true;

        private Color _lightningColorCache;
        public Color LightningColor
        {
            get
            {
                TreatColorChange();
                return _lightningColorCache;
            }
        }

        private Color _foreColorCache;
        public Color ForeColor
        {
            get
            {
                TreatColorChange();
                return _foreColorCache;
            }
        }

        private Color _backColorCache;
        public Color BackColor
        {
            get
            {
                TreatColorChange();
                return _backColorCache;
            }
        }

        private Brush _foreBrushCache;
        public Brush ForeBrush
        {
            get
            {
                TreatColorChange();
                return _foreBrushCache;
            }
        }

        private Brush _backBrushCache;
        public Brush BackBrush
        {
            get
            {
                TreatColorChange();
                return _backBrushCache;
            }
        }

        private Color GetCurrentLightningColor()
        {
            var status = Status as TwitterStatus;
            if (status != null)
            {
                var ptv = this.ParentTabViewModel.TabProperty;

                if (Setting.Instance.ColoringProperty.MyCurrentTweet.IsActivated &&
                    TwitterHelper.IsMyCurrentTweet(this, ptv))
                    return Setting.Instance.ColoringProperty.MyCurrentTweet.GetColor();

                if (Setting.Instance.ColoringProperty.MySubTweet.IsActivated & 
                    TwitterHelper.IsMyTweet(this))
                    return Setting.Instance.ColoringProperty.MySubTweet.GetColor();
                
                if (Setting.Instance.ColoringProperty.InReplyToMeCurrent.IsActivated &&
                    TwitterHelper.IsInReplyToMeCurrent(this, ptv))
                    return Setting.Instance.ColoringProperty.InReplyToMeCurrent.GetColor();
                
                if (Setting.Instance.ColoringProperty.InReplyToMeSub.IsActivated && 
                    TwitterHelper.IsInReplyToMe(this))
                    return Setting.Instance.ColoringProperty.InReplyToMeSub.GetColor();

                var uvm = UserStorage.Get(this.Status.User);

                if (Setting.Instance.ColoringProperty.Friend.IsActivated && 
                    TwitterHelper.IsFollowingCurrent(uvm, ptv) &&
                    TwitterHelper.IsFollowerCurrent(uvm, ptv))
                    return Setting.Instance.ColoringProperty.Friend.GetColor();
                
                if (Setting.Instance.ColoringProperty.Following.IsActivated && 
                    TwitterHelper.IsFollowingCurrent(uvm, ptv))
                    return Setting.Instance.ColoringProperty.Following.GetColor();
                
                if (Setting.Instance.ColoringProperty.Follower.IsActivated && 
                    TwitterHelper.IsFollowerCurrent(uvm, ptv))
                    return Setting.Instance.ColoringProperty.Follower.GetColor();
                
                if (Setting.Instance.ColoringProperty.Friend.IsActivated &&
                    TwitterHelper.IsFollowingCurrent(uvm, ptv) &&
                    TwitterHelper.IsFollowerCurrent(uvm, ptv))
                    return Setting.Instance.ColoringProperty.Friend.GetColor();
                
                if (Setting.Instance.ColoringProperty.Following.IsActivated &&
                    TwitterHelper.IsFollowingCurrent(uvm, ptv))
                    return Setting.Instance.ColoringProperty.Following.GetColor();
                
                if (Setting.Instance.ColoringProperty.Follower.IsActivated &&
                    TwitterHelper.IsFollowerCurrent(uvm, ptv))
                    return Setting.Instance.ColoringProperty.Follower.GetColor();
                
                if (Setting.Instance.ColoringProperty.FriendAny.IsActivated && 
                    TwitterHelper.IsFollowingAny(uvm) &&
                    TwitterHelper.IsFollowerAny(uvm))
                    return Setting.Instance.ColoringProperty.FriendAny.GetColor();
                
                if (Setting.Instance.ColoringProperty.FollowingAny.IsActivated &&
                    TwitterHelper.IsFollowingAny(uvm))
                    return Setting.Instance.ColoringProperty.FollowingAny.GetColor();
                
                if (Setting.Instance.ColoringProperty.FollowerAny.IsActivated &&
                    TwitterHelper.IsFollowerAny(uvm))
                    return Setting.Instance.ColoringProperty.FollowerAny.GetColor();
                
                return Setting.Instance.ColoringProperty.BaseHighlightColor.GetColor();
            }
            else
            {
                if (Setting.Instance.ColoringProperty.DirectMessage.Activated)
                    return Setting.Instance.ColoringProperty.DirectMessage.GetColor(false);
                else
                    return Setting.Instance.ColoringProperty.BaseHighlightColor.GetColor();
            }
        }

        private Color GetCurrentCommonColor(bool dark)
        {

            var pts = ParentTabViewModel.SelectedTweetViewModel;
            if ((Setting.Instance.ColoringProperty.Selected.IsDarkActivated ||
                Setting.Instance.ColoringProperty.Selected.IsLightActivated) &&
                pts != null && pts.Status.User.NumericId == Status.User.NumericId &&
                pts.Status.Id != Status.Id)
            {
                var dm = Status as TwitterDirectMessage;
                if (dm != null)
                {
                    return RoutePairColor(dark,
                        Setting.Instance.ColoringProperty.Selected,
                        Setting.Instance.ColoringProperty.DirectMessage,
                        Setting.Instance.ColoringProperty.BaseColor);
                }
                else
                {
                    if (TwitterHelper.IsPublishedByRetweet(this))
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.Selected,
                            Setting.Instance.ColoringProperty.Retweeted,
                            Setting.Instance.ColoringProperty.BaseColor);
                    }
                    else
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.Selected,
                            Setting.Instance.ColoringProperty.BaseColor);
                    }
                }
            }
            else
            {
                var dm = Status as TwitterDirectMessage;
                if (dm != null)
                {
                    return RoutePairColor(dark,
                        Setting.Instance.ColoringProperty.DirectMessage,
                        Setting.Instance.ColoringProperty.BaseColor);
                }
                else
                {
                    if (TwitterHelper.IsPublishedByRetweet(this))
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.Retweeted,
                            Setting.Instance.ColoringProperty.BaseColor);
                    }
                    else
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.BaseColor);
                    }
                }
            }
        }

        private Color RoutePairColor(bool dark, params IPairColorElement[] colorProps)
        {
            if (dark)
                return RoutePairColorDark(colorProps);
            else
                return RoutePairColor(colorProps);
        }

        private Color RoutePairColor(params IPairColorElement[] colorProps)
        {
            return colorProps.Where((b) => b.IsLightActivated).Select((b) => b.GetLightColor()).First();
        }

        private Color RoutePairColorDark(params IPairColorElement[] colorProps)
        {
            return colorProps.Where((b) => b.IsDarkActivated).Select((b) => b.GetDarkColor()).First();
        }

        #endregion

        #region Color changing

        private static StackTaskDispatcher taskDispatcher;

        private void TreatColorChange()
        {
            bool change = isColorChanged;
            isColorChanged = false;
            bool lchanged = lightningColorChanged;
            lightningColorChanged = false;
            if (change)
            {
                // 色の更新があった
                taskDispatcher.Push(() => CommitColorChanged(lchanged));
            }
        }

        /// <summary>
        /// このTweetViewModelの色設定を更新します。
        /// </summary>
        private void CommitColorChanged(bool lightningColorUpdated)
        {
            bool nlf = false;
            if (lightningColorUpdated)
            {
                var nlc = GetCurrentLightningColor();
                if (_lightningColorCache != nlc)
                {
                    _lightningColorCache = nlc;
                    nlf = true;
                }
            }

            bool bcf = false;
            var bcc = GetCurrentCommonColor(false);
            if (_backColorCache != bcc)
            {
                _backColorCache = bcc;
                _backBrushCache = new SolidColorBrush(_backColorCache);
                _backBrushCache.Freeze();
                bcf = true;
            }

            bool fcf = false;
            var fcc = GetCurrentCommonColor(true);
            if (_foreColorCache != fcc)
            {
                _foreColorCache = fcc;
                _foreBrushCache = new SolidColorBrush(_foreColorCache);
                _foreBrushCache.Freeze();
                fcf = true;
            }
            if (nlf)
            {
                RaisePropertyChanged(() => LightningColor);
            }
            if (bcf)
            {
                RaisePropertyChanged(() => BackColor);
                RaisePropertyChanged(() => BackBrush);
            }
            if (fcf)
            {
                RaisePropertyChanged(() => ForeColor);
                RaisePropertyChanged(() => ForeBrush);
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
            Browser.Start("http://twitter.com/" + Status.User.ScreenName + "/status/" + Status.Id.ToString());
        }

        #endregion



        #endregion
    }
}
