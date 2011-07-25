using System;
using System.Linq;
using System.Threading.Tasks;
using Dulcet.Twitter.Rest;
using Inscribe.Common;
using Inscribe.Communication;
using Inscribe.Configuration;
using Inscribe.Filter;
using Inscribe.Filter.Filters.Numeric;
using Inscribe.Storage;
using Inscribe.ViewModels.Behaviors.Messaging;
using Livet;
using Livet.Commands;

namespace Inscribe.ViewModels.PartBlocks.MainBlock
{
    public class UserPageViewModel : TimelineCoreViewModelBase
    {
        public TabViewModel Parent { get; private set; }

        public override bool IsActive
        {
            get { return Parent.CurrentForegroundTimeline == this; }
        }

        public override void InvalidateIsActive()
        {
            RaisePropertyChanged(() => IsActive);
        }

        public UserPageViewModel(TabViewModel parent, string userId)
        {
            this.Parent = parent;
            this._timelineListViewModel = new TimelineListViewModel(parent);
            this.SetUser(userId);
        }

        private TimelineListViewModel _timelineListViewModel;

        public TimelineListViewModel TimelineListViewModel
        {
            get { return _timelineListViewModel; }
        }

        private UserViewModel _user = null;
        public UserViewModel User
        {
            get { return _user; }
            set
            {
                if (value == null ?
                    _user == null :
                    _user != null && _user.TwitterUser.ScreenName == value.TwitterUser.ScreenName)
                    return;
                _user = value;
                RaisePropertyChanged(() => User);
                RaisePropertyChanged(() => UserProfileImageUrl);
                RaisePropertyChanged(() => ScreenName);
                RaisePropertyChanged(() => Name);
                RaisePropertyChanged(() => IsVerified);
                RaisePropertyChanged(() => IsProtected);
                RaisePropertyChanged(() => Location);
                RaisePropertyChanged(() => ProfileUrl);
                RaisePropertyChanged(() => Bio);
                RaisePropertyChanged(() => Tweets);
                RaisePropertyChanged(() => Favorites);
                RaisePropertyChanged(() => Following);
                RaisePropertyChanged(() => Followers);
                RaisePropertyChanged(() => Listed);
                SetUserTimeline();
            }
        }

        public bool _isCommunicating = false;
        public bool IsCommunicating
        {
            get { return _isCommunicating; }
            set
            {
                _isCommunicating = value;
                RaisePropertyChanged(() => IsCommunicating);
            }
        }

        public Uri UserProfileImageUrl
        {
            get
            {
                return User != null ? User.TwitterUser.ProfileImage : null;
            }
        }

        public string ScreenName
        {
            get
            {
                return User != null ? User.TwitterUser.ScreenName : String.Empty;
            }
        }

        public string Name
        {
            get
            {
                return User != null ? User.TwitterUser.UserName : String.Empty;
            }
        }

        public bool IsVerified
        {
            get
            {
                return User != null && User.TwitterUser.IsVerified;
            }
        }

        public bool IsProtected
        {
            get
            {
                return User != null && User.TwitterUser.IsProtected;
            }
        }

        public string Location
        {
            get
            {
                return User != null ? User.TwitterUser.Location : String.Empty;
            }
        }

        public string ProfileUrl
        {
            get
            {
                return User != null ? User.TwitterUser.Web : String.Empty;
            }
        }


        #region OpenLinkCommand
        DelegateCommand<string> _OpenLinkCommand;

        public DelegateCommand<string> OpenLinkCommand
        {
            get
            {
                if (_OpenLinkCommand == null)
                    _OpenLinkCommand = new DelegateCommand<string>(OpenLink);
                return _OpenLinkCommand;
            }
        }

        private void OpenLink(string parameter)
        {
            if (String.IsNullOrEmpty(parameter))
                Browser.Start("http://twitter.com/" + User.TwitterUser.ScreenName);
            else
                Browser.Start("http://twitter.com/" + User.TwitterUser.ScreenName + "/" + parameter);
        }
        #endregion


        #region OpenUserWebCommand
        DelegateCommand _OpenUserWebCommand;

        public DelegateCommand OpenUserWebCommand
        {
            get
            {
                if (_OpenUserWebCommand == null)
                    _OpenUserWebCommand = new DelegateCommand(OpenUserWeb);
                return _OpenUserWebCommand;
            }
        }

        private void OpenUserWeb()
        {
            if (User != null && User.TwitterUser.Web != null)
                Browser.Start(User.TwitterUser.Web);
        }
        #endregion

        public string Bio
        {
            get
            {
                return User != null ? User.TwitterUser.Bio : String.Empty;
            }
        }

        public string Tweets
        {
            get
            {
                return User != null ? User.TwitterUser.Tweets.ToString() : null;
            }
        }

        public string Favorites
        {
            get
            {
                return User != null ? User.TwitterUser.Favorites.ToString() : String.Empty;
            }
        }

        private bool _inputMode = false;
        public bool InputMode
        {
            get { return _inputMode; }
            private set
            {
                _inputMode = value;
                RaisePropertyChanged(() => InputMode);
            }
        }

        private string _editScreenName = String.Empty;
        public string EditScreenName
        {
            get { return _editScreenName; }
            set
            {
                _editScreenName = value;
                RaisePropertyChanged(() => EditScreenName);
            }
        }

        public string Following
        {
            get
            {
                return User != null ? User.TwitterUser.Followings.ToString() : null;
            }
        }

        public string Followers
        {
            get
            {
                return User != null ? User.TwitterUser.Followers.ToString() : null;
            }
        }

        public string Listed
        {
            get
            {
                return User != null ? User.TwitterUser.Listed.ToString() : null;
            }
        }


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
            var desc = "@" + this.User.TwitterUser.ScreenName;
            var filt = new[]{ new FilterCluster(){
                 Filters = new[]{new FilterUserId(this.User.TwitterUser.NumericId)}}};

            switch (Setting.Instance.TimelineExperienceProperty.UserOpenTransition)
            {
                case Configuration.Settings.TransitionMethod.ViewStack:
                    this.Parent.AddTopTimeline(filt);
                    break;
                case Configuration.Settings.TransitionMethod.AddTab:
                    this.Parent.Parent.AddTab(
                        new Configuration.Tabs.TabProperty()
                        {
                            Name = desc,
                            TweetSources = filt
                        });
                    break;
                case Configuration.Settings.TransitionMethod.AddColumn:
                    var col = this.Parent.Parent.Parent.CreateColumn();
                    col.AddTab(new Configuration.Tabs.TabProperty()
                    {
                        Name = desc,
                        TweetSources = filt
                    });
                    break;
            }
        }
        #endregion


        #region ReceiveTimelineCommand
        DelegateCommand _ReceiveTimelineCommand;

        public DelegateCommand ReceiveTimelineCommand
        {
            get
            {
                if (_ReceiveTimelineCommand == null)
                    _ReceiveTimelineCommand = new DelegateCommand(ReceiveTimeline);
                return _ReceiveTimelineCommand;
            }
        }

        private void ReceiveTimeline()
        {
            if (User == null) return;
            IsCommunicating = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var acc = AccountStorage.GetRandom(a => a.Followings.Contains(this.User.TwitterUser.NumericId), true);
                    var tweets = ApiHelper.ExecApi(() => acc.GetUserTimeline(userId: this.User.TwitterUser.NumericId, count: 100));
                    if (tweets != null)
                        tweets.ForEach(t => TweetStorage.Register(t));
                }
                catch (Exception e)
                {
                    ExceptionStorage.Register(e, ExceptionCategory.TwitterError, "ユーザータイムラインを受信できませんでした: @" + this.User.TwitterUser.ScreenName, ReceiveTimeline);
                }
                finally
                {
                    IsCommunicating = false;
                }
            });
        }
        #endregion

        private void SetUserTimeline()
        {
            if (User != null)
            {
                this.TimelineListViewModel.Sources = new[] { new FilterUserId(this.User.TwitterUser.NumericId) };
                this.TimelineListViewModel.Commit(true);
            }
        }

        public event Action CloseRequired = () => { };


        #region ManageFollowCommand
        DelegateCommand _ManageFollowCommand;

        public DelegateCommand ManageFollowCommand
        {
            get
            {
                if (_ManageFollowCommand == null)
                    _ManageFollowCommand = new DelegateCommand(ManageFollow);
                return _ManageFollowCommand;
            }
        }

        private void ManageFollow()
        {
            // todo:implementation
        }
        #endregion

        #region EditUserCommand
        DelegateCommand _EditUserCommand;

        public DelegateCommand EditUserCommand
        {
            get
            {
                if (_EditUserCommand == null)
                    _EditUserCommand = new DelegateCommand(EditUser);
                return _EditUserCommand;
            }
        }

        private void EditUser()
        {
            EditScreenName = ScreenName;
            InputMode = true;
            this.Messenger.Raise(new Livet.Messaging.InteractionMessage("FocusToInput"));
        }
        #endregion

        #region EditFinishCommand
        DelegateCommand<bool> _EditFinishCommand;

        public DelegateCommand<bool> EditFinishCommand
        {
            get
            {
                if (_EditFinishCommand == null)
                    _EditFinishCommand = new DelegateCommand<bool>(EditFinish);
                return _EditFinishCommand;
            }
        }

        private void EditFinish(bool parameter)
        {
            if (parameter && !String.IsNullOrEmpty(EditScreenName))
                this.SetUser(EditScreenName);
            else
                InputMode = false;
        }
        #endregion

        /// <summary>
        /// ユーザーを設定します。<para />
        /// NullかString.Emptyが指定されると、ユーザー編集モードに入ります。
        /// </summary>
        internal void SetUser(string screenName)
        {
            if (String.IsNullOrEmpty(screenName))
            {
                User = null;
                InputMode = true;
                this.Messenger.Raise(new Livet.Messaging.InteractionMessage("FocusToInput"));
            }
            else
            {
                InputMode = false;
                screenName = screenName.TrimStart('@', ' ', '\t');
                this.IsCommunicating = true;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var user = UserStorage.Lookup(screenName);
                        if (user == null)
                        {
                            var cred = AccountStorage.GetRandom();
                            if (cred != null)
                            {
                                this.User = UserStorage.Get(ApiHelper.ExecApi(() => cred.GetUserByScreenName(screenName)));
                            }
                        }
                        DispatcherHelper.BeginInvoke(() =>
                            {
                                IsCommunicating = false;
                                if (user != null)
                                    User = user;
                                else
                                    this.Messenger.Raise(new Livet.Messaging.InformationMessage(
                                        "ユーザー @" + screenName + "の情報を取得できません。",
                                        "ユーザー情報取得エラー", System.Windows.MessageBoxImage.Warning,
                                        "InformationMessage"));
                            });
                    }
                    finally
                    {
                        IsCommunicating = false;
                    }
                });
            }
        }

        #region GetFocusCommand
        DelegateCommand _GetFocusCommand;

        public DelegateCommand GetFocusCommand
        {
            get
            {
                if (_GetFocusCommand == null)
                    _GetFocusCommand = new DelegateCommand(GetFocus);
                return _GetFocusCommand;
            }
        }

        private void GetFocus()
        {
            this.OnGetFocus();
        }
        #endregion

        public override void SetSelect(ListSelectionKind kind)
        {
            Messenger.Raise(new SetListSelectionMessage("SetListSelection", kind));
        }

        public override TimelineChild.TabDependentTweetViewModel SelectedTweetViewModel
        {
            get
            {
                return this.TimelineListViewModel.SelectedTweetViewModel;
            }
            set
            {
                this.TimelineListViewModel.SelectedTweetViewModel = value;
            }
        }

        public override void Commit(bool reinvalidate)
        {
            this.TimelineListViewModel.Commit(reinvalidate);
        }
    }

}