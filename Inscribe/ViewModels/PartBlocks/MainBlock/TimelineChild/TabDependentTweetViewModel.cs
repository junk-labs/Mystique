using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dulcet.Twitter;
using Dulcet.Twitter.Rest;
using Inscribe.Common;
using Inscribe.Communication.Posting;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.Filter;
using Inscribe.Filter.Filters.Numeric;
using Inscribe.Filter.Filters.Particular;
using Inscribe.Storage;
using Inscribe.Text;
using Inscribe.ViewModels.Dialogs;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Inscribe.Subsystems;
using Inscribe.ViewModels.Behaviors.Messaging;
using Inscribe.Core;
using Inscribe.Filter.Filters.Text;

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
                    _nameBackColorCache = Setting.Instance.ColoringProperty.DefaultNameColor.GetColor();
                    _lightColorCache = Setting.Instance.ColoringProperty.DefaultColor.GetDarkColor();
                    _darkColorCache = Setting.Instance.ColoringProperty.DefaultColor.GetLightColor();
                    _textColorCache = Setting.Instance.ColoringProperty.DefaultTextColor.GetColor();
                    _nameBackBrushCache = new SolidColorBrush(_nameBackColorCache).CloneFreeze();
                    _lightBrushCache = new SolidColorBrush(_lightColorCache).CloneFreeze();
                    _darkBrushCache = new SolidColorBrush(_darkColorCache).CloneFreeze();
                    _textBrushCache = new SolidColorBrush(_textColorCache).CloneFreeze();
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

        public bool IsTextSelected
        {
            get { return !String.IsNullOrEmpty(_selectedText); }
        }

        public bool IsSelected
        {
            get { return Parent.CurrentForegroundTimeline.SelectedTweetViewModel == this; }
        }

        private string _selectedText = null;
        public string SelectedText
        {
            get { return _selectedText; }
            set
            {
                _selectedText = value;
                RaisePropertyChanged(() => SelectedText);
                RaisePropertyChanged(() => IsTextSelected);
            }
        }

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return this._isExpanded; }
            set
            {
                this._isExpanded = value;
                RaisePropertyChanged(() => IsExpanded);
            }
        }

        #endregion

        public void SettingValueChanged()
        {
            System.Diagnostics.Debug.WriteLine("setting value changed.");
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
                this.nameBackColorChanged = true;
            // RaisePropertyChanged(() => DarkBrush);
            // more speedy
            // DarkかLightを見てるはずなので両方Raiseしとけば、まぁ。
            RaisePropertyChanged("DarkBrush");
            RaisePropertyChanged("LightBrush");
        }

        #region Coloring Property

        private bool nameBackColorChanged = true;
        private bool isColorChanged = true;

        private Color _nameBackColorCache;
        public Color NameBackColor
        {
            get
            {
                TreatColorChange();
                return _nameBackColorCache;
            }
        }

        private Color _lightColorCache;
        public Color LightColor
        {
            get
            {
                TreatColorChange();
                return _lightColorCache;
            }
        }

        private Color _darkColorCache;
        public Color DarkColor
        {
            get
            {
                TreatColorChange();
                return _darkColorCache;
            }
        }

        private Brush _lightBrushCache;
        public Brush LightBrush
        {
            get
            {
                TreatColorChange();
                return _lightBrushCache;
            }
        }

        private Brush _darkBrushCache;
        public Brush DarkBrush
        {
            get
            {
                TreatColorChange();
                return _darkBrushCache;
            }
        }

        private Brush _nameBackBrushCache;
        public Brush NameBackBrush
        {
            get
            {
                TreatColorChange();
                return _nameBackBrushCache;
            }
        }

        private Color _textColorCache;
        public Color TextColor
        {
            get
            {
                TreatColorChange();
                return _textColorCache;
            }
        }

        private Brush _textBrushCache;
        public Brush TextBrush
        {
            get
            {
                TreatColorChange();
                return _textBrushCache;
            }
        }

        private Color GetCurrentNameBackColor()
        {
            var status = this.Tweet.Status as TwitterStatus;
            var ptv = this.Parent.TabProperty;
            if (status != null)
            {

                if (Setting.Instance.ColoringProperty.MyColor.IsActivated &&
                    TwitterHelper.IsMyCurrentTweet(this.Tweet, ptv))
                    return Setting.Instance.ColoringProperty.MyColor.GetColor();

                var uvm = UserStorage.Get(TwitterHelper.GetSuggestedUser(this.Tweet.Status));

                if (Setting.Instance.ColoringProperty.FriendColor.IsActivated &&
                    TwitterHelper.IsFollowingCurrent(uvm, ptv) &&
                    TwitterHelper.IsFollowerCurrent(uvm, ptv))
                    return Setting.Instance.ColoringProperty.FriendColor.GetColor();

                if (Setting.Instance.ColoringProperty.FollowingColor.IsActivated &&
                    TwitterHelper.IsFollowingCurrent(uvm, ptv))
                    return Setting.Instance.ColoringProperty.FollowingColor.GetColor();

                if (Setting.Instance.ColoringProperty.FollowerColor.IsActivated &&
                    TwitterHelper.IsFollowerCurrent(uvm, ptv))
                    return Setting.Instance.ColoringProperty.FollowerColor.GetColor();

                return Setting.Instance.ColoringProperty.DefaultNameColor.GetColor();
            }
            else
            {
                if (Setting.Instance.ColoringProperty.DirectMessageNameColor.IsActivated)
                    return Setting.Instance.ColoringProperty.DirectMessageNameColor.GetColor();
                else
                    return Setting.Instance.ColoringProperty.DefaultNameColor.GetColor();
            }
        }

        private Color GetCurrentCommonColor(bool dark)
        {
            try
            {
                var pts = Parent.CurrentForegroundTimeline.SelectedTweetViewModel;
                if ((Setting.Instance.ColoringProperty.SelectedColor.IsDarkActivated ||
                    Setting.Instance.ColoringProperty.SelectedColor.IsLightActivated) &&
                    pts != null && pts.Tweet.Status.User.NumericId == this.Tweet.Status.User.NumericId &&
                    pts.Tweet.Status.Id != this.Tweet.Status.Id)
                {
                    if (this.Tweet.Status is TwitterDirectMessage)
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.SelectedColor,
                            Setting.Instance.ColoringProperty.DirectMessageColor,
                            Setting.Instance.ColoringProperty.DefaultColor);
                    }
                    else if (TwitterHelper.IsPublishedByRetweet(this.Tweet))
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.SelectedColor,
                            Setting.Instance.ColoringProperty.RetweetedColor,
                            Setting.Instance.ColoringProperty.DefaultColor);
                    }
                    else if (TwitterHelper.IsMentionOfMe(this.Tweet.Status))
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.SelectedColor,
                            Setting.Instance.ColoringProperty.MentionColor,
                            Setting.Instance.ColoringProperty.DefaultColor);
                    }
                    else
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.SelectedColor,
                            Setting.Instance.ColoringProperty.DefaultColor);
                    }
                }
                else
                {
                    if (this.Tweet.Status is TwitterDirectMessage)
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.DirectMessageColor,
                            Setting.Instance.ColoringProperty.DefaultColor);
                    }
                    else if (TwitterHelper.IsPublishedByRetweet(this.Tweet))
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.RetweetedColor,
                            Setting.Instance.ColoringProperty.DefaultColor);
                    }
                    else if (TwitterHelper.IsMentionOfMe(this.Tweet.Status))
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.MentionColor,
                            Setting.Instance.ColoringProperty.DefaultColor);
                    }
                    else
                    {
                        return RoutePairColor(dark,
                            Setting.Instance.ColoringProperty.DefaultColor);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.ConfigurationError,
                    "色設定が破損しています。再試行を押すと、色設定を再作成します。",
                    () => Setting.Instance.ColoringProperty = new ColoringProperty());
                return Colors.Black;
            }
        }

        private Color GetCurrentTextColor()
        {
            try
            {
                var pts = Parent.CurrentForegroundTimeline.SelectedTweetViewModel;
                if (Setting.Instance.ColoringProperty.SelectedTextColor.IsActivated &&
                    pts != null && pts.Tweet.Status.User.NumericId == this.Tweet.Status.User.NumericId &&
                    pts.Tweet.Status.Id != this.Tweet.Status.Id)
                    return Setting.Instance.ColoringProperty.SelectedTextColor.GetColor();

                if (Setting.Instance.ColoringProperty.DirectMessageTextColor.IsActivated &&
                    this.Tweet.Status is TwitterDirectMessage)
                    return Setting.Instance.ColoringProperty.DirectMessageTextColor.GetColor();
                if (Setting.Instance.ColoringProperty.RetweetedTextColor.IsActivated &&
                    TwitterHelper.IsPublishedByRetweet(this.Tweet))
                    return Setting.Instance.ColoringProperty.RetweetedTextColor.GetColor();


                if (Setting.Instance.ColoringProperty.MentionTextColor.IsActivated &&
                    TwitterHelper.IsMentionOfMe(this.Tweet.Status))
                    return Setting.Instance.ColoringProperty.MentionTextColor.GetColor();

                return Setting.Instance.ColoringProperty.DefaultTextColor.GetColor();
            }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.ConfigurationError,
                    "色設定が破損しています。再試行を押すと、色設定を再作成します。",
                    () => Setting.Instance.ColoringProperty = new ColoringProperty());
                return Colors.Black;
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
            taskDispatcher = new StackTaskDispatcher(1);
            ThreadHelper.Halt += () => taskDispatcher.Dispose();
        }

        private static StackTaskDispatcher taskDispatcher;

        private bool pending = false;

        private void TreatColorChange()
        {
            bool change = isColorChanged;
            isColorChanged = false;
            bool lchanged = nameBackColorChanged;
            nameBackColorChanged = false;
            if (change && !pending)
            {
                pending = true;
                // 色の更新があった
                taskDispatcher.Push(() => CommitColorChanged(lchanged));
            }
        }

        /// <summary>
        /// このTweetViewModelの色設定を更新します。
        /// </summary>
        private void CommitColorChanged(bool nameBackColorUpdated)
        {
            pending = false;
            bool nlf = false;
            if (nameBackColorUpdated)
            {
                var nlc = GetCurrentNameBackColor();
                if (_nameBackColorCache != nlc)
                {
                    _nameBackColorCache = nlc;
                    _nameBackBrushCache = new SolidColorBrush(_nameBackColorCache).CloneFreeze();
                    nlf = true;
                }
            }

            bool bcf = false;
            var bcc = GetCurrentCommonColor(true);
            if (_darkColorCache != bcc)
            {
                _darkColorCache = bcc;
                _darkBrushCache = new SolidColorBrush(_darkColorCache).CloneFreeze();
                bcf = true;
            }

            bool fcf = false;
            var fcc = GetCurrentCommonColor(false);
            if (_lightColorCache != fcc)
            {
                _lightColorCache = fcc;
                _lightBrushCache = new SolidColorBrush(_lightColorCache).CloneFreeze();
                fcf = true;
            }

            bool tcf = false;
            var tcc = GetCurrentTextColor();
            if (_textColorCache != tcc)
            {
                _textColorCache = tcc;
                _textBrushCache = new SolidColorBrush(_textColorCache).CloneFreeze();
                tcf = true;
            }

            if (nlf)
            {
                RaisePropertyChanged(() => NameBackColor);
                RaisePropertyChanged(() => NameBackBrush);
            }
            if (bcf)
            {
                RaisePropertyChanged(() => DarkColor);
                RaisePropertyChanged(() => DarkBrush);
            }
            if (fcf)
            {
                RaisePropertyChanged(() => LightColor);
                RaisePropertyChanged(() => LightBrush);
            }
            if (tcf)
            {
                RaisePropertyChanged(() => TextColor);
                RaisePropertyChanged(() => TextBrush);
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
        ViewModelCommand _ShowUserDetailCommand;

        public ViewModelCommand ShowUserDetailCommand
        {
            get
            {
                if (_ShowUserDetailCommand == null)
                    _ShowUserDetailCommand = new ViewModelCommand(ShowUserDetail);
                return _ShowUserDetailCommand;
            }
        }

        private void ShowUserDetail()
        {
            this.Parent.AddTopUser(TwitterHelper.GetSuggestedUser(this.Tweet).ScreenName);
        }
        #endregion

        #region RetweetedUserDetailCommand
        ViewModelCommand _RetweetedUserDetailCommand;

        public ViewModelCommand RetweetedUserDetailCommand
        {
            get
            {
                if (_RetweetedUserDetailCommand == null)
                    _RetweetedUserDetailCommand = new ViewModelCommand(RetweetedUserDetail);
                return _RetweetedUserDetailCommand;
            }
        }

        private void RetweetedUserDetail()
        {
            this.Parent.AddTopUser(this.Tweet.Status.User.ScreenName);
        }
        #endregion

        #region DirectMessageReceipientDetailCommand
        ViewModelCommand _DirectMessageReceipientDetailCommand;

        public ViewModelCommand DirectMessageReceipientDetailCommand
        {
            get
            {
                if (_DirectMessageReceipientDetailCommand == null)
                    _DirectMessageReceipientDetailCommand = new ViewModelCommand(DirectMessageReceipientDetail);
                return _DirectMessageReceipientDetailCommand;
            }
        }

        private void DirectMessageReceipientDetail()
        {
            this.Parent.AddTopUser(((TwitterDirectMessage)this.Tweet.Status).Recipient.ScreenName);
        }
        #endregion

        #region OpenConversationCommand
        ViewModelCommand _OpenConversationCommand;

        public ViewModelCommand OpenConversationCommand
        {
            get
            {
                if (_OpenConversationCommand == null)
                    _OpenConversationCommand = new ViewModelCommand(OpenConversation);
                return _OpenConversationCommand;
            }
        }

        private void OpenConversation()
        {
            var s = this.Tweet.Status as TwitterStatus;
            if (s == null)
                return;
            if (s.InReplyToStatusId == 0)
                s = s.RetweetedOriginal;
            if (s == null || s.InReplyToStatusId == 0)
                return;
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
        ViewModelCommand _OpenDMConversationCommand;

        public ViewModelCommand OpenDMConversationCommand
        {
            get
            {
                if (_OpenDMConversationCommand == null)
                    _OpenDMConversationCommand = new ViewModelCommand(OpenDMConversation);
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

        #region ShowRetweetedOriginalCommand
        ViewModelCommand _ShowRetweetedOriginalCommand;

        public ViewModelCommand ShowRetweetedOriginalCommand
        {
            get
            {
                if (_ShowRetweetedOriginalCommand == null)
                    _ShowRetweetedOriginalCommand = new ViewModelCommand(ShowRetweetedOriginal);
                return _ShowRetweetedOriginalCommand;
            }
        }

        private void ShowRetweetedOriginal()
        {
            this.Parent.AddTopTimeline(new[]{ 
                new Filter.Filters.Numeric.FilterStatusId(((TwitterStatus)this.Tweet.Status).RetweetedOriginal.Id)
            });
        }
        #endregion

        #region SurfaceClickCommand
        private Livet.Commands.ViewModelCommand _SurfaceClickCommand;

        public Livet.Commands.ViewModelCommand SurfaceClickCommand
        {
            get
            {
                if (_SurfaceClickCommand == null)
                {
                    _SurfaceClickCommand = new Livet.Commands.ViewModelCommand(SurfaceClick, () => !Setting.Instance.TweetExperienceProperty.UseFullLineView);
                }
                return _SurfaceClickCommand;
            }
        }

        public void SurfaceClick()
        {
            // TODO:暫定
            DeselectCommand.Execute();
        }

        #endregion

        #endregion

        #region Context Menu Commands

        #region SelectAllCommand
        private ViewModelCommand _SelectAllCommand;

        public ViewModelCommand SelectAllCommand
        {
            get
            {
                if (_SelectAllCommand == null)
                {
                    _SelectAllCommand = new ViewModelCommand(SelectAll);
                }
                return _SelectAllCommand;
            }
        }

        public void SelectAll()
        {
            this.Messenger.Raise(new RichTextBoxMessage("RichTextBoxAction", RichTextActionType.SelectAll));
        }
        #endregion

        #region CopyCommand
        private Livet.Commands.ViewModelCommand _CopyCommand;

        public Livet.Commands.ViewModelCommand CopyCommand
        {
            get
            {
                if (_CopyCommand == null)
                {
                    _CopyCommand = new Livet.Commands.ViewModelCommand(Copy);
                }
                return _CopyCommand;
            }
        }

        public void Copy()
        {
            this.Messenger.Raise(new RichTextBoxMessage("RichTextBoxAction", RichTextActionType.Copy));
        }
        #endregion

        #region SearchInKrileCommand
        private ViewModelCommand _SearchInKrileCommand;

        public ViewModelCommand SearchInKrileCommand
        {
            get
            {
                if (_SearchInKrileCommand == null)
                {
                    _SearchInKrileCommand = new ViewModelCommand(SearchInKrile);
                }
                return _SearchInKrileCommand;
            }
        }

        public void SearchInKrile()
        {
            this.Parent.AddTopTimeline(
                new[] { new Inscribe.Filter.Filters.Text.FilterText(this.SelectedText) });
        }
        #endregion


        #region SearchGoogleCommand
        private ViewModelCommand _SearchGoogleCommand;

        public ViewModelCommand SearchGoogleCommand
        {
            get
            {
                if (_SearchGoogleCommand == null)
                {
                    _SearchGoogleCommand = new ViewModelCommand(SearchGoogle);
                }
                return _SearchGoogleCommand;
            }
        }

        public void SearchGoogle()
        {
            Browser.Start(
                "http://www.google.co.jp/search?q=" + this.SelectedText);
        }
        #endregion

        #endregion

        #region Timeline Action Commands

        #region MentionCommand
        ViewModelCommand _MentionCommand;

        public ViewModelCommand MentionCommand
        {
            get
            {
                if (_MentionCommand == null)
                    _MentionCommand = new ViewModelCommand(Mention);
                return _MentionCommand;
            }
        }

        private void Mention()
        {
            if (Setting.Instance.MouseAssignProperty.IsMouseAssignEnabled)
            {
                if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.ReplyActionSet.ControlKeyAction);
                else if(Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.ReplyActionSet.AltKeyAction);
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.ReplyActionSet.ShiftKeyAction);
                else
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.ReplyActionSet.NoneKeyAction);
            }
            else
            {
                this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetOpenText(true, true);
                this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetInReplyTo(this.Tweet);
            }
        }
        #endregion

        #region FavoriteCommand
        ViewModelCommand _FavoriteCommand;

        public ViewModelCommand FavoriteCommand
        {
            get
            {
                if (_FavoriteCommand == null)
                    _FavoriteCommand = new ViewModelCommand(FavoriteInternal, CanFavoriteInternal);
                return _FavoriteCommand;
            }
        }

        private bool CanFavoriteInternal()
        {
            return this.Tweet.CanFavorite;
        }

        private void FavoriteInternal()
        {
            if (Setting.Instance.MouseAssignProperty.IsMouseAssignEnabled)
            {
                if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.FavActionSet.ControlKeyAction);
                else if(Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.FavActionSet.AltKeyAction);
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.FavActionSet.ShiftKeyAction);
                else
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.FavActionSet.NoneKeyAction);
            }
            else
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    FavoriteMultiUser();
                else
                    ToggleFavorite();
            }
        }
        #endregion


        public void ToggleFavorite()
        {
            if (!this.Tweet.CanFavorite) return;
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
            if (!this.Tweet.CanFavorite) return;
            PostOffice.FavTweet(this.Parent.TabProperty.LinkAccountInfos, this.Tweet);
        }

        public void Unfavorite()
        {
            if (!this.Tweet.CanFavorite) return;
            PostOffice.UnfavTweet(this.Parent.TabProperty.LinkAccountInfos, this.Tweet);
        }

        #region FavoriteMultiUserCommand
        ViewModelCommand _FavoriteMultiUserCommand;

        public ViewModelCommand FavoriteMultiUserCommand
        {
            get
            {
                if (_FavoriteMultiUserCommand == null)
                    _FavoriteMultiUserCommand = new ViewModelCommand(FavoriteMultiUser, CanFavoriteMultiUser);
                return _FavoriteMultiUserCommand;
            }
        }

        private bool CanFavoriteMultiUser()
        {
            return this.Tweet.CanFavorite;
        }

        private void FavoriteMultiUser()
        {
            if (!this.Tweet.CanFavorite) return;
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
        ViewModelCommand _RetweetCommand;

        public ViewModelCommand RetweetCommand
        {
            get
            {
                if (_RetweetCommand == null)
                    _RetweetCommand = new ViewModelCommand(RetweetInternal);
                return _RetweetCommand;
            }
        }

        private void RetweetInternal()
        {
            if (Setting.Instance.MouseAssignProperty.IsMouseAssignEnabled)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.RetweetActionSet.ControlKeyAction);
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.RetweetActionSet.AltKeyAction);
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.RetweetActionSet.ShiftKeyAction);
                else
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.RetweetActionSet.NoneKeyAction);
            }
            else
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    RetweetMultiUser();
                else
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
        ViewModelCommand _RetweetMultiUserCommand;

        public ViewModelCommand RetweetMultiUserCommand
        {
            get
            {
                if (_RetweetMultiUserCommand == null)
                    _RetweetMultiUserCommand = new ViewModelCommand(RetweetMultiUser);
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
        ViewModelCommand _UnofficialRetweetCommand;

        public ViewModelCommand UnofficialRetweetCommand
        {
            get
            {
                if (_UnofficialRetweetCommand == null)
                    _UnofficialRetweetCommand = new ViewModelCommand(UnofficialRetweet);
                return _UnofficialRetweetCommand;
            }
        }

        private void UnofficialRetweet()
        {
            if (Setting.Instance.MouseAssignProperty.IsMouseAssignEnabled)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.UnofficialRetweetActionSet.ControlKeyAction);
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.UnofficialRetweetActionSet.AltKeyAction);
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.UnofficialRetweetActionSet.ShiftKeyAction);
                else
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.UnofficialRetweetActionSet.NoneKeyAction);
            }
            else
            {
                if (this.Tweet.Status is TwitterDirectMessage) return;
                this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetOpenText(true, true);
                var status = this.Tweet.Status;
                if (status is TwitterStatus && ((TwitterStatus)status).RetweetedOriginal != null)
                    status = ((TwitterStatus)status).RetweetedOriginal;
                this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetText(" RT @" + status.User.ScreenName + ": " + status.Text);
                this.Parent.Parent.Parent.Parent.InputBlockViewModel.SetInputCaretIndex(0);
            }
        }
        #endregion
      
        #region QuoteCommand
        ViewModelCommand _QuoteCommand;

        public ViewModelCommand QuoteCommand
        {
            get
            {
                if (_QuoteCommand == null)
                    _QuoteCommand = new ViewModelCommand(Quote);
                return _QuoteCommand;
            }
        }

        private void Quote()
        {
            if (Setting.Instance.MouseAssignProperty.IsMouseAssignEnabled)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.QuoteTweetActionSet.ControlKeyAction);
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.QuoteTweetActionSet.AltKeyAction);
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.QuoteTweetActionSet.ShiftKeyAction);
                else
                    MouseAssignCore.ExecuteAction(this.Tweet, Setting.Instance.MouseAssignProperty.QuoteTweetActionSet.NoneKeyAction);
            }
            else
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
        }
        #endregion

        #region DeleteCommand
        ViewModelCommand _DeleteCommand;

        public ViewModelCommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                    _DeleteCommand = new ViewModelCommand(Delete);
                return _DeleteCommand;
            }
        }

        private void Delete()
        {
            if (!this.Tweet.ShowDeleteButton) return;
            var conf = new ConfirmationMessage("ツイート @" + this.Tweet.Status.User.ScreenName + ": " + this.Tweet.Status.Text + " を削除してもよろしいですか？", 
                "ツイートの削除", System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxButton.OKCancel, "Confirm");
            this.RaiseMessage(conf);
            if (conf.Response.GetValueOrDefault())
            {
                PostOffice.RemoveTweet(AccountStorage.Get(this.Tweet.Status.User.ScreenName), this.Tweet.Status.Id);
            }
        }
        #endregion

        #region ReportForSpamCommand
        ViewModelCommand _ReportForSpamCommand;

        public ViewModelCommand ReportForSpamCommand
        {
            get
            {
                if (_ReportForSpamCommand == null)
                    _ReportForSpamCommand = new ViewModelCommand(ReportForSpam, CanReportForSpam);
                return _ReportForSpamCommand;
            }
        }

        private bool CanReportForSpam()
        {
            return !this.Tweet.IsMyTweet ||
                Setting.Instance.TimelineExperienceProperty.CanBlockMyself;
        }

        private void ReportForSpam()
        {
            if (!CanReportForSpam()) return;
            var conf = new ConfirmationMessage("ユーザー @" + this.Tweet.Status.User.ScreenName + " をスパム報告してもよろしいですか？" + Environment.NewLine +
                "(Krileに存在するすべてのアカウントでスパム報告を行います)",
                "スパム報告の確認", System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxButton.OKCancel, "Confirm");
            this.RaiseMessage(conf);
            if (conf.Response.GetValueOrDefault())
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
        ViewModelCommand _DeselectCommand;

        public ViewModelCommand DeselectCommand
        {
            get
            {
                if (_DeselectCommand == null)
                    _DeselectCommand = new ViewModelCommand(Deselect);
                return _DeselectCommand;
            }
        }

        private void Deselect()
        {
            this.Parent.CurrentForegroundTimeline.SelectedTweetViewModel = null;
        }
        #endregion

        #region ClickUserIconCommand
        ViewModelCommand _ClickUserIconCommand;

        public ViewModelCommand ClickUserIconCommand
        {
            get
            {
                if (_ClickUserIconCommand == null)
                    _ClickUserIconCommand = new ViewModelCommand(ClickUserIcon);
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
        ViewModelCommand _DirectMessageCommand;

        public ViewModelCommand DirectMessageCommand
        {
            get
            {
                if (_DirectMessageCommand == null)
                    _DirectMessageCommand = new ViewModelCommand(DirectMessage);
                return _DirectMessageCommand;
            }
        }

        private void DirectMessage()
        {
            this.Parent.Parent.Parent.Parent.InputBlockViewModel
                .SetOpenText(true, true);
            this.Parent.Parent.Parent.Parent.InputBlockViewModel
                .SetText("d @" + this.Tweet.Status.User.ScreenName + " ");
            this.Parent.Parent.Parent.Parent.InputBlockViewModel
                .SetInputCaretIndex(this.Parent.Parent.Parent.Parent.InputBlockViewModel.CurrentInputDescription.InputText.Length);
        }
        #endregion

        #region MuteCommand
        ViewModelCommand _MuteCommand;

        public ViewModelCommand MuteCommand
        {
            get
            {
                if (_MuteCommand == null)
                    _MuteCommand = new ViewModelCommand(Mute);
                return _MuteCommand;
            }
        }

        private void Mute()
        {
            var mvm = new MuteViewModel(this.Tweet, this.SelectedText);
            this.RaiseMessage(new TransitionMessage(mvm, "Mute"));
        }
        #endregion

        #region OpenUserCommand
        ListenerCommand<UserViewModel> _OpenUserCommand;

        public ListenerCommand<UserViewModel> OpenUserCommand
        {
            get
            {
                if (_OpenUserCommand == null)
                    _OpenUserCommand = new ListenerCommand<UserViewModel>(OpenUser);
                return _OpenUserCommand;
            }
        }

        private void OpenUser(UserViewModel parameter)
        {
            this.Parent.AddTopUser(parameter.TwitterUser.ScreenName);
        }

        #endregion

        #endregion

        #region Key assign helper

        public void OpenIndexOfUrl(int index)
        {
            var m = RegularExpressions.UrlRegex.Matches(this.Tweet.TweetText)
                .Cast<Match>().Skip(index).FirstOrDefault();
            if (m == null) return;
            Browser.Start(m.Value);
        }

        public void OpenIndexOfAction(int index)
        {
            Action a;
            if ((a = GenerateActions(this.Tweet.TweetText).Skip(index).FirstOrDefault()) != null)
                a();
        }

        public static IEnumerable<Action> GenerateActions(string text)
        {
            foreach (var tok in Tokenizer.Tokenize(text))
            {
                var ctt = tok.Text;
                switch (tok.Kind)
                {
                    case TokenKind.AtLink:
                        yield return new Action(() =>
                        {
                            if (KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn != null &&
                                KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn.SelectedTabViewModel != null)
                                KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn
                                    .SelectedTabViewModel.AddTopUser(ctt);
                        });
                        break;
                    case TokenKind.Hashtag:
                        yield return new Action(() =>
                        {
                            if (KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn != null &&
                                KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn.SelectedTabViewModel != null)
                                KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn
                                    .SelectedTabViewModel.AddTopTimeline(new[] { new FilterText(ctt) });
                        });
                        break;
                    case TokenKind.Url:
                        yield return new Action(() =>
                        {
                            Browser.Start(ctt);
                        });
                        break;
                }
            }
        }

        #endregion

        private void RaiseMessage(InteractionMessage message)
        {
            this.Messenger.Raise(message);
        }
    }
}
