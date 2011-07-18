using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Dulcet.Twitter;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.Storage;
using Inscribe.Threading;
using Livet;
using Livet.Commands;

namespace Inscribe.ViewModels.Timeline
{
    public class TabDependentTweetViewModel : ViewModel
    {
        public TabViewModel Parent { get; private set; }

        public TweetViewModel Tweet { get; private set; }
        
        public TabDependentTweetViewModel(TweetViewModel tvm, TabViewModel parent)
        {
            if (tvm == null)
                throw new ArgumentNullException("tvm");
            if (parent == null)
                throw new ArgumentNullException("parent");
            this.Parent = parent;
            this.Tweet = tvm;

            switch (Setting.Instance.TimelineExperienceProperty.TimelineItemInitStrategy)
            {
                case ItemInitStrategy.None:
                    break;
                case ItemInitStrategy.DefaultColors:
                    _lightningColorCache = Setting.Instance.ColoringProperty.BaseHighlightColor.GetColor();
                    _foreColorCache = Setting.Instance.ColoringProperty.BaseColor.GetDarkColor();
                    _backColorCache = Setting.Instance.ColoringProperty.BaseColor.GetLightColor();
                    _foreBrushCache = new SolidColorBrush(_foreColorCache);
                    _foreBrushCache.Freeze();
                    _backBrushCache = new SolidColorBrush(_backColorCache);
                    _backBrushCache.Freeze();
                    break;
                case ItemInitStrategy.Full:
                    CommitColorChanged(true);
                    break;
            }
        }

        #region Binding helper
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
        #endregion

        public void SettingValueChanged()
        {
            PendingColorChanged(true);
            this.Tweet.SettingValueChanged();
        }

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
            var status = this.Tweet.Status as TwitterStatus;
            var ptv = this.Parent.TabProperty;
            if (status != null)
            {

                if (Setting.Instance.ColoringProperty.MyCurrentTweet.IsActivated &&
                    TwitterHelper.IsMyCurrentTweet(this.Tweet, ptv))
                    return Setting.Instance.ColoringProperty.MyCurrentTweet.GetColor();

                if (Setting.Instance.ColoringProperty.MySubTweet.IsActivated &
                    TwitterHelper.IsMyTweet(this.Tweet))
                    return Setting.Instance.ColoringProperty.MySubTweet.GetColor();

                if (Setting.Instance.ColoringProperty.InReplyToMeCurrent.IsActivated &&
                    TwitterHelper.IsInReplyToMeCurrent(this.Tweet, ptv))
                    return Setting.Instance.ColoringProperty.InReplyToMeCurrent.GetColor();

                if (Setting.Instance.ColoringProperty.InReplyToMeSub.IsActivated &&
                    TwitterHelper.IsInReplyToMe(this.Tweet))
                    return Setting.Instance.ColoringProperty.InReplyToMeSub.GetColor();

                var uvm = UserStorage.Get(this.Tweet.Status.User);

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
            var pts = Parent.CurrentForegroundTimeline.SelectedTweetViewModel;
            if ((Setting.Instance.ColoringProperty.Selected.IsDarkActivated ||
                Setting.Instance.ColoringProperty.Selected.IsLightActivated) &&
                pts != null && pts.Tweet.Status.User.NumericId == this.Tweet.Status.User.NumericId &&
                pts.Tweet.Status.Id != this.Tweet.Status.Id)
            {
                var dm = this.Tweet.Status as TwitterDirectMessage;
                if (dm != null)
                {
                    return RoutePairColor(dark,
                        Setting.Instance.ColoringProperty.Selected,
                        Setting.Instance.ColoringProperty.DirectMessage,
                        Setting.Instance.ColoringProperty.BaseColor);
                }
                else
                {
                    if (TwitterHelper.IsPublishedByRetweet(this.Tweet))
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
                var dm = this.Tweet.Status as TwitterDirectMessage;
                if (dm != null)
                {
                    return RoutePairColor(dark,
                        Setting.Instance.ColoringProperty.DirectMessage,
                        Setting.Instance.ColoringProperty.BaseColor);
                }
                else
                {
                    if (TwitterHelper.IsPublishedByRetweet(this.Tweet))
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

        static TabDependentTweetViewModel()
        {
            taskDispatcher = new StackTaskDispatcher(10);
            ThreadHelper.Halt += () => taskDispatcher.Dispose();
        }

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

        public override bool Equals(object obj)
        {
            var tdtv = obj as TabDependentTweetViewModel;
            if (tdtv != null)
                return this.Tweet.Equals(tdtv.Tweet);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Tweet.GetHashCode();
        }

        #region Timeline Action Commands

        #region MentionCommand
        DelegateCommand _MentionCommand;

        public DelegateCommand MentionCommand
        {
            get
            {
                if (_MentionCommand == null)
                    _MentionCommand = new DelegateCommand(Mention);
                return _MentionCommand;
            }
        }

        private void Mention()
        {
        }
        #endregion

        #region FavoriteCommand
        DelegateCommand _FavoriteCommand;

        public DelegateCommand FavoriteCommand
        {
            get
            {
                if (_FavoriteCommand == null)
                    _FavoriteCommand = new DelegateCommand(Favorite);
                return _FavoriteCommand;
            }
        }

        private void Favorite()
        {
            // TODO:Implementation
        }
        #endregion

        #region FavoriteMultiUserCommand
        DelegateCommand _FavoriteMultiUserCommand;

        public DelegateCommand FavoriteMultiUserCommand
        {
            get
            {
                if (_FavoriteMultiUserCommand == null)
                    _FavoriteMultiUserCommand = new DelegateCommand(FavoriteMultiUser);
                return _FavoriteMultiUserCommand;
            }
        }

        private void FavoriteMultiUser()
        {
            // TODO:Implementation
        }
        #endregion

        #region RetweetCommand
        DelegateCommand _RetweetCommand;

        public DelegateCommand RetweetCommand
        {
            get
            {
                if (_RetweetCommand == null)
                    _RetweetCommand = new DelegateCommand(Retweet);
                return _RetweetCommand;
            }
        }

        private void Retweet()
        {
            // TODO:Implementation
        }
        #endregion

        #region RetweetMultiUserCommand
        DelegateCommand _RetweetMultiUserCommand;

        public DelegateCommand RetweetMultiUserCommand
        {
            get
            {
                if (_RetweetMultiUserCommand == null)
                    _RetweetMultiUserCommand = new DelegateCommand(RetweetMultiUser);
                return _RetweetMultiUserCommand;
            }
        }

        private void RetweetMultiUser()
        {
            // TODO:Implementation
        }
        #endregion

        #region UnofficialRetweetCommand
        DelegateCommand _UnofficialRetweetCommand;

        public DelegateCommand UnofficialRetweetCommand
        {
            get
            {
                if (_UnofficialRetweetCommand == null)
                    _UnofficialRetweetCommand = new DelegateCommand(UnofficialRetweet);
                return _UnofficialRetweetCommand;
            }
        }

        private void UnofficialRetweet()
        {
            // TODO:Implementation
        }
        #endregion
      
        #region QuoteCommand
        DelegateCommand _QuoteCommand;

        public DelegateCommand QuoteCommand
        {
            get
            {
                if (_QuoteCommand == null)
                    _QuoteCommand = new DelegateCommand(Quote);
                return _QuoteCommand;
            }
        }

        private void Quote()
        {
            // TODO:Implementation
        }
        #endregion

        #region DeleteCommand
        DelegateCommand _DeleteCommand;

        public DelegateCommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                    _DeleteCommand = new DelegateCommand(Delete);
                return _DeleteCommand;
            }
        }

        private void Delete()
        {
            // TODO:Implementation
        }
        #endregion

        #region ReportAsSpamCommand
        DelegateCommand _ReportAsSpamCommand;

        public DelegateCommand ReportAsSpamCommand
        {
            get
            {
                if (_ReportAsSpamCommand == null)
                    _ReportAsSpamCommand = new DelegateCommand(ReportAsSpam);
                return _ReportAsSpamCommand;
            }
        }

        private void ReportAsSpam()
        {
            // TODO:Implementation
        }
        #endregion
      
        #region DeselectCommand
        DelegateCommand _DeselectCommand;

        public DelegateCommand DeselectCommand
        {
            get
            {
                if (_DeselectCommand == null)
                    _DeselectCommand = new DelegateCommand(Deselect);
                return _DeselectCommand;
            }
        }

        private void Deselect()
        {
            this.Parent.CurrentForegroundTimeline.SelectedTweetViewModel = null;
        }
        #endregion

        #region CreateUserTabCommand
        DelegateCommand _CreateUserTabCommand;

        public DelegateCommand CreateUserTabCommand
        {
            get
            {
                if (_CreateUserTabCommand == null)
                    _CreateUserTabCommand = new DelegateCommand(CreateUserTab);
                return _CreateUserTabCommand;
            }
        }

        private void CreateUserTab()
        {
            // TODO:Implementation
        }
        #endregion

        #endregion
    }
}
