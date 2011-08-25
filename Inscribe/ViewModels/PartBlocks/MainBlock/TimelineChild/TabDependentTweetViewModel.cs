using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Dulcet.Twitter;
using Dulcet.Twitter.Rest;
using Inscribe.Common;
using Inscribe.Communication;
using Inscribe.Communication.Posting;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.Filter;
using Inscribe.Filter.Filters.Numeric;
using Inscribe.Filter.Filters.Particular;
using Inscribe.Storage;
using Inscribe.Threading;
using Inscribe.ViewModels.Dialogs;
using Livet;
using Livet.Commands;
using Livet.Messaging;

namespace Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild
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

        private bool _isTextSelected = false;
        public bool IsTextSelected
        {
            get { return _isTextSelected; }
            set
            {
                _isTextSelected = value;
                RaisePropertyChanged(() => IsTextSelected);
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

        #region Navigation Commands

        #region ShowUserDetailCommand
        DelegateCommand _ShowUserDetailCommand;

        public DelegateCommand ShowUserDetailCommand
        {
            get
            {
                if (_ShowUserDetailCommand == null)
                    _ShowUserDetailCommand = new DelegateCommand(ShowUserDetail);
                return _ShowUserDetailCommand;
            }
        }

        private void ShowUserDetail()
        {
            this.Parent.AddTopUser(TwitterHelper.GetSuggestedUser(this.Tweet).ScreenName);
        }
        #endregion

        #region RetweetedUserDetailCommand
        DelegateCommand _RetweetedUserDetailCommand;

        public DelegateCommand RetweetedUserDetailCommand
        {
            get
            {
                if (_RetweetedUserDetailCommand == null)
                    _RetweetedUserDetailCommand = new DelegateCommand(RetweetedUserDetail);
                return _RetweetedUserDetailCommand;
            }
        }

        private void RetweetedUserDetail()
        {
            this.Parent.AddTopUser(this.Tweet.Status.User.ScreenName);
        }
        #endregion

        #region DirectMessageReceipientDetailCommand
        DelegateCommand _DirectMessageReceipientDetailCommand;

        public DelegateCommand DirectMessageReceipientDetailCommand
        {
            get
            {
                if (_DirectMessageReceipientDetailCommand == null)
                    _DirectMessageReceipientDetailCommand = new DelegateCommand(DirectMessageReceipientDetail);
                return _DirectMessageReceipientDetailCommand;
            }
        }

        private void DirectMessageReceipientDetail()
        {
            this.Parent.AddTopUser(((TwitterDirectMessage)this.Tweet.Status).Recipient.ScreenName);
        }
        #endregion

        #region OpenConversationCommand
        DelegateCommand _OpenConversationCommand;

        public DelegateCommand OpenConversationCommand
        {
            get
            {
                if (_OpenConversationCommand == null)
                    _OpenConversationCommand = new DelegateCommand(OpenConversation);
                return _OpenConversationCommand;
            }
        }

        private void OpenConversation()
        {
            var s = this.Tweet.Status as TwitterStatus;
            if (s == null || s.InReplyToStatusId == 0) return;
            IEnumerable<IFilter> filter = null;
            string description = String.Empty;
            if (Setting.Instance.TimelineExperienceProperty.IsShowConversationAsTree)
            {
                filter = new[] { new FilterMentionTree(this.Tweet.Status.Id) };
                description = "@#" + this.Tweet.Status.Id.ToString();
            }
            else
            {
                filter = new[] { new FilterConversation(this.Tweet.Status.User.ScreenName, ((TwitterStatus)this.Tweet.Status).InReplyToUserScreenName) };
                description = "Cv:@" + this.Tweet.Status.User.ScreenName + "&@" + ((TwitterStatus)this.Tweet.Status).InReplyToUserScreenName;
            }
            switch (Setting.Instance.TimelineExperienceProperty.ConversationTransition)
            {
                case TransitionMethod.ViewStack:
                    this.Parent.AddTopTimeline(filter);
                    break;
                case TransitionMethod.AddTab:
                    this.Parent.Parent.AddTab(new Configuration.Tabs.TabProperty()
                    {
                        Name = description,
                        TweetSources = filter
                    });
                    break;
                case TransitionMethod.AddColumn:
                    var column = this.Parent.Parent.Parent.CreateColumn();
                    column.AddTab(new Configuration.Tabs.TabProperty()
                    {
                        Name = description,
                        TweetSources = filter
                    });
                    break;
            }
        }
        #endregion

        #region OpenDMConversationCommand
        DelegateCommand _OpenDMConversationCommand;

        public DelegateCommand OpenDMConversationCommand
        {
            get
            {
                if (_OpenDMConversationCommand == null)
                    _OpenDMConversationCommand = new DelegateCommand(OpenDMConversation);
                return _OpenDMConversationCommand;
            }
        }

        private void OpenDMConversation()
        {
            var filter = new[] { new FilterConversation(this.Tweet.Status.User.ScreenName, ((TwitterDirectMessage)this.Tweet.Status).Recipient.ScreenName) };
            var description = "DM:@" + this.Tweet.Status.User.ScreenName + "&@" + ((TwitterDirectMessage)this.Tweet.Status).Recipient.ScreenName;
            switch (Setting.Instance.TimelineExperienceProperty.ConversationTransition)
            {
                case TransitionMethod.ViewStack:
                    this.Parent.AddTopTimeline(filter);
                    break;
                case TransitionMethod.AddTab:
                    this.Parent.Parent.AddTab(new Configuration.Tabs.TabProperty()
                    {
                        Name = description,
                        TweetSources = filter
                    });
                    break;
                case TransitionMethod.AddColumn:
                    var column = this.Parent.Parent.Parent.CreateColumn();
                    column.AddTab(new Configuration.Tabs.TabProperty()
                    {
                        Name = description,
                        TweetSources = filter
                    });
                    break;
            }
        }
        #endregion
      
        #endregion

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
            this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetOpenText(true, true);
            this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetInReplyTo(this.Tweet);
        }
        #endregion

        #region FavoriteCommand
        DelegateCommand _FavoriteCommand;

        public DelegateCommand FavoriteCommand
        {
            get
            {
                if (_FavoriteCommand == null)
                    _FavoriteCommand = new DelegateCommand(FavoriteInternal);
                return _FavoriteCommand;
            }
        }

        private void FavoriteInternal()
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                FavoriteMultiUser();
            }
            else
            {
                ToggleFavorite();

            }
        }
        #endregion

        public void ToggleFavorite()
        {
            if (this.Tweet.Status is TwitterDirectMessage) return;
            if (this.Parent.TabProperty.LinkAccountInfos.Select(ai => ai.UserViewModel)
                .All(u => this.Tweet.FavoredUsers.Contains(u)))
            {
                // all account favored
                Unfavorite();
            }
            else
            {
                Favorite();
            }
        }

        public void Favorite()
        {
            if (this.Tweet.Status is TwitterDirectMessage) return;
            PostOffice.FavTweet(this.Parent.TabProperty.LinkAccountInfos, this.Tweet);
        }

        public void Unfavorite()
        {
            if (this.Tweet.Status is TwitterDirectMessage) return;
            PostOffice.UnfavTweet(this.Parent.TabProperty.LinkAccountInfos, this.Tweet);
        }

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
            var favored = AccountStorage.Accounts.Where(a => this.Tweet.FavoredUsers.Contains(a.UserViewModel)).ToArray();
            this.Parent.Parent.Parent.Parent.SelectUser(ModalParts.SelectionKind.Favorite,
                favored,
                u =>
                {
                    PostOffice.FavTweet(u.Except(favored), this.Tweet);
                    PostOffice.UnfavTweet(favored.Except(u), this.Tweet);
                });
        }
        #endregion

        #region RetweetCommand
        DelegateCommand _RetweetCommand;

        public DelegateCommand RetweetCommand
        {
            get
            {
                if (_RetweetCommand == null)
                    _RetweetCommand = new DelegateCommand(RetweetInternal);
                return _RetweetCommand;
            }
        }

        private void RetweetInternal()
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                RetweetMultiUser();
            }
            else
            {
                ToggleRetweet();
            }
        }
        #endregion

        public void ToggleRetweet()
        {
            if (this.Tweet.Status is TwitterDirectMessage) return;
            if (this.Parent.TabProperty.LinkAccountInfos.Select(ai => ai.UserViewModel)
                .All(u => this.Tweet.RetweetedUsers.Contains(u)))
            {
                // all account favored
                Unretweet();
            }
            else
            {
                Retweet();
            }
        }

        public void Retweet()
        {
            if (this.Tweet.Status is TwitterDirectMessage) return;
            PostOffice.Retweet(this.Parent.TabProperty.LinkAccountInfos, this.Tweet);
        }

        public void Unretweet()
        {
            if (this.Tweet.Status is TwitterDirectMessage) return;
            PostOffice.Unretweet(this.Parent.TabProperty.LinkAccountInfos, this.Tweet);
        }

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
            var retweeted = AccountStorage.Accounts.Where(a => this.Tweet.RetweetedUsers.Contains(a.UserViewModel)).ToArray();
            this.Parent.Parent.Parent.Parent.SelectUser(ModalParts.SelectionKind.Retweet,
                retweeted,
                u =>
                {
                    PostOffice.Retweet(u.Except(retweeted), this.Tweet);
                    PostOffice.Unretweet(retweeted.Except(u), this.Tweet);
                });
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
            if (this.Tweet.Status is TwitterDirectMessage) return;
            this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetOpenText(true, true);
            var status = this.Tweet.Status;
            if(status is TwitterStatus && ((TwitterStatus)status).RetweetedOriginal != null)
                status = ((TwitterStatus)status).RetweetedOriginal;
            this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetText(" RT @" + status.User.ScreenName + ": " + status.Text);
            this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetInputCaretIndex(0);
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
            if (this.Tweet.Status is TwitterDirectMessage) return;
            this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetOpenText(true, true);
            this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetInReplyTo(null);
            this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetInReplyTo(this.Tweet);
            var status = this.Tweet.Status;
            if (status is TwitterStatus && ((TwitterStatus)status).RetweetedOriginal != null)
                status = ((TwitterStatus)status).RetweetedOriginal;
            this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetText(" QT @" + status.User.ScreenName + ": " + status.Text);
            this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetInputCaretIndex(0);

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
            if (!this.Tweet.ShowDeleteButton) return;
            var conf = new ConfirmationMessage("ツイート @" + this.Tweet.Status.User.ScreenName + ": " + this.Tweet.Status.Text + " を削除してもよろしいですか？", 
                "ツイートの削除", System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxButton.OKCancel, "Confirm");
            this.Messenger.Raise(conf);
            if (conf.Response)
            {
                PostOffice.RemoveTweet(AccountStorage.Get(this.Tweet.Status.User.ScreenName), this.Tweet.Status.Id);
            }
        }
        #endregion

        #region ReportForSpamCommand
        DelegateCommand _ReportForSpamCommand;

        public DelegateCommand ReportForSpamCommand
        {
            get
            {
                if (_ReportForSpamCommand == null)
                    _ReportForSpamCommand = new DelegateCommand(ReportForSpam);
                return _ReportForSpamCommand;
            }
        }

        private void ReportForSpam()
        {
            var conf = new ConfirmationMessage("ユーザー @" + this.Tweet.Status.User.ScreenName + " をスパム報告してもよろしいですか？" + Environment.NewLine +
                "(Krileに存在するすべてのアカウントでスパム報告を行います)",
                "スパム報告の確認", System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxButton.OKCancel, "Confirm");
            this.Messenger.Raise(conf);
            if (conf.Response)
            {
                AccountStorage.Accounts.ForEach(i => Task.Factory.StartNew(() => ApiHelper.ExecApi(() => i.ReportSpam(this.Tweet.Status.User.NumericId))));
                TweetStorage.Remove(this.Tweet.Status.Id);
                NotifyStorage.Notify("R4Sしました: @" + this.Tweet.Status.User.ScreenName);
                Task.Factory.StartNew(() =>
                    TweetStorage.GetAll(t => t.Status.User.NumericId == this.Tweet.Status.User.NumericId)
                    .ForEach(vm => TweetStorage.Remove(vm.Status.Id)));
            }
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

        #region ClickUserIconCommand
        DelegateCommand _ClickUserIconCommand;

        public DelegateCommand ClickUserIconCommand
        {
            get
            {
                if (_ClickUserIconCommand == null)
                    _ClickUserIconCommand = new DelegateCommand(ClickUserIcon);
                return _ClickUserIconCommand;
            }
        }

        private void ClickUserIcon()
        {
            var user = TwitterHelper.GetSuggestedUser(this.Tweet);
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                var cluster = new FilterCluster(){
                    ConcatenateAnd = false,
                    Negate = false,
                    Filters = this.Parent.TabProperty.TweetSources.ToArray()};
                this.Parent.TabProperty.TweetSources = new[]{ cluster.Restrict(new FilterCluster(){ ConcatenateAnd = false, Negate = true, Filters = new[]{ new FilterUserId(user.NumericId) }}).Optimize()}.ToArray();
                Task.Factory.StartNew(() => this.Parent.BaseTimeline.CoreViewModel.InvalidateCache());
            }
            else
            {
                var filter = new[] { new FilterUserId(user.NumericId) };
                var desc = "@" + user.ScreenName;
                switch (Setting.Instance.TimelineExperienceProperty.UserExtractTransition)
                {
                    case TransitionMethod.ViewStack:
                        this.Parent.AddTopTimeline(filter);
                        break;
                    case TransitionMethod.AddTab:
                        this.Parent.Parent.AddTab(new Configuration.Tabs.TabProperty() { Name = desc, TweetSources = filter });
                        break;
                    case TransitionMethod.AddColumn:
                        var column = this.Parent.Parent.Parent.CreateColumn();
                        column.AddTab(new Configuration.Tabs.TabProperty() { Name = desc, TweetSources = filter });
                        break;
                }
            }
        }
        #endregion

        #region DirectMessageCommand
        DelegateCommand _DirectMessageCommand;

        public DelegateCommand DirectMessageCommand
        {
            get
            {
                if (_DirectMessageCommand == null)
                    _DirectMessageCommand = new DelegateCommand(DirectMessage);
                return _DirectMessageCommand;
            }
        }

        private void DirectMessage()
        {
            this.Parent.Parent.Parent.Parent.InputBlockViewModel
                .SetText("d @" + this.Tweet.Status.User.ScreenName + " ");
            this.Parent.Parent.Parent.Parent.InputBlockViewModel
                .SetInputCaretIndex(this.Parent.Parent.Parent.Parent.InputBlockViewModel.CurrentInputDescription.InputText.Length);
        }
        #endregion

        #region MuteCommand
        DelegateCommand _MuteCommand;

        public DelegateCommand MuteCommand
        {
            get
            {
                if (_MuteCommand == null)
                    _MuteCommand = new DelegateCommand(Mute);
                return _MuteCommand;
            }
        }

        private void Mute()
        {
            var mvm = new MuteViewModel(this.Tweet);
            this.Messenger.Raise(new TransitionMessage(mvm, "Mute"));
        }
        #endregion

        #region OpenUserCommand
        DelegateCommand<UserViewModel> _OpenUserCommand;

        public DelegateCommand<UserViewModel> OpenUserCommand
        {
            get
            {
                if (_OpenUserCommand == null)
                    _OpenUserCommand = new DelegateCommand<UserViewModel>(OpenUser);
                return _OpenUserCommand;
            }
        }

        private void OpenUser(UserViewModel parameter)
        {
            this.Parent.AddTopUser(parameter.TwitterUser.ScreenName);
        }

        #endregion

        #endregion
    }
}
