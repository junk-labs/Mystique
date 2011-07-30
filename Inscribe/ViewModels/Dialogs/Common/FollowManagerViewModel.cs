﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Inscribe.Model;
using Inscribe.ViewModels.PartBlocks.MainBlock;
using Dulcet.Twitter.Rest;
using System.Threading.Tasks;
using Inscribe.Storage;
using Inscribe.Communication;

namespace Inscribe.ViewModels.Dialogs.Common
{
    public class FollowManagerViewModel : ViewModel
    {
        public UserViewModel Target { get; private set; }

        public FollowManagerViewModel(UserViewModel target)
        {
            this.Target = target;
        }

        private bool _isCommunicating = false;
        public bool IsCommunicating
        {
            get { return _isCommunicating; }
            set
            {
                _isCommunicating = value;
                RaisePropertyChanged(() => IsCommunicating);
            }
        }

        public void ApplyFollowing(Action callback)
        {
            IsCommunicating = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Parallel.ForEach(Relations, r => r.CommitRelation());
                }
                finally
                {
                    DispatcherHelper.BeginInvoke(() =>
                        {
                            IsCommunicating = false;
                            callback();
                        });
                }
            });
        }

        public Uri TargetProfileImageUrl
        {
            get
            {
                return Target.TwitterUser.ProfileImage;
            }
        }

        public string TargetScreenName
        {
            get
            {
                return Target.TwitterUser.ScreenName;
            }
        }

        private IEnumerable<Relation> _relations = null;
        public IEnumerable<Relation> Relations
        {
            get
            {
                if (_relations == null)
                    _relations = AccountStorage.Accounts
                        .Where(e => e.ScreenName != Target.TwitterUser.ScreenName)
                        .Select(a => new Relation(a, Target)).ToList();
                return _relations;
            }
        }

        public event Action CloseRequired = () => { };


        #region CommitCommand
        DelegateCommand _CommitCommand;

        public DelegateCommand CommitCommand
        {
            get
            {
                if (_CommitCommand == null)
                    _CommitCommand = new DelegateCommand(Commit);
                return _CommitCommand;
            }
        }

        private void Commit()
        {
            ApplyFollowing(()=>
                Messenger.Raise(new WindowActionMessage("WindowAction", WindowAction.Close)));
        }
        #endregion

        #region CancelCommand
        DelegateCommand _CancelCommand;

        public DelegateCommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                    _CancelCommand = new DelegateCommand(Cancel);
                return _CancelCommand;
            }
        }

        private void Cancel()
        {
            Messenger.Raise(new WindowActionMessage("WindowAction", WindowAction.Close));
        }
        #endregion
      
    }


    public class Relation : ViewModel
    {
        public AccountInfo Info { get; private set; }

        public UserViewModel TargetUser { get; private set; }

        private bool _isStandby = true;
        public bool IsStandby
        {
            get { return _isStandby; }
            set
            {
                _isStandby = value;
                RaisePropertyChanged(() => IsStandby);
            }
        }

        public enum EnumValue
        {
            Value = 1,
            Value2 = 3,

        }

        public Relation(AccountInfo info, UserViewModel user)
        {
            this.Info = info;
            this.TargetUser = user;
            IsStandby = true;
            RefreshUserData();
        }

        public void CommitRelation()
        {
            if (OrigState == State || IsStandby) return;
            var id = TargetUser.TwitterUser.NumericId;
            try
            {
                switch (State)
                {
                    case FollowState.Following:
                        if (OrigState == FollowState.Blocking)
                        {
                            ApiHelper.ExecApi(() => Info.DestroyBlockUser(userId: id));
                            Info.RemoveBlocking(id);
                        }
                        ApiHelper.ExecApi(()=>Info.CreateFriendship(userId: id));
                        Info.RegisterFollowing(id);
                        break;
                    case FollowState.Unfollowing:
                        if (OrigState == FollowState.Blocking)
                        {
                            ApiHelper.ExecApi(() => Info.DestroyBlockUser(userId: id));
                            Info.RemoveBlocking(id);
                        }
                        ApiHelper.ExecApi(() => Info.DestroyFriendship(userId: id));
                        Info.RemoveFollowing(id);
                        break;
                    case FollowState.Blocking:
                        ApiHelper.ExecApi(() => Info.CreateBlockUser(userId: id));
                        Info.RegisterBlocking(id);
                        break;
                    case FollowState.ReportForSpam:
                        ApiHelper.ExecApi(() => Info.ReportSpam(userId: id));
                        Info.RegisterBlocking(id);
                        break;
                }
            }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.TwitterError, "フォロー情報の更新ができませんでした。(@" + Info.ScreenName + ")");
            }
        }

        public enum FollowState
        {
            Following,
            Unfollowing,
            Blocking,
            ReportForSpam
        }

        public Uri UserProfileImageUrl
        {
            get { return Info.ProfileImage; }
        }

        public string UserScreenName
        {
            get { return Info.ScreenName; }
        }

        public bool IsFollowbacked
        {
            get { return Info.IsFollowedBy(TargetUser.TwitterUser.NumericId); }
        }

        public FollowState OrigState { get; private set; }

        private FollowState _state = FollowState.Unfollowing;
        public FollowState State
        {
            get { return _state; }
            set
            {
                _state = value;
                RaisePropertyChanged(() => State);
                RaisePropertyChanged(() => IsFollowing);
                RaisePropertyChanged(() => Blocking);
            }
        }

        public bool IsFollowing
        {
            get
            {
                return State == FollowState.Following;
            }
        }

        public bool Blocking
        {
            get
            {
                return State == FollowState.Blocking || State == FollowState.ReportForSpam;
            }
        }


        #region FollowCommand
        DelegateCommand _FollowCommand;

        public DelegateCommand FollowCommand
        {
            get
            {
                if (_FollowCommand == null)
                    _FollowCommand = new DelegateCommand(Follow);
                return _FollowCommand;
            }
        }

        private void Follow()
        {
            State = FollowState.Following;
        }
        #endregion


        #region RemoveCommand
        DelegateCommand _RemoveCommand;

        public DelegateCommand RemoveCommand
        {
            get
            {
                if (_RemoveCommand == null)
                    _RemoveCommand = new DelegateCommand(Remove);
                return _RemoveCommand;
            }
        }

        private void Remove()
        {
            State = FollowState.Unfollowing;
        }
        #endregion


        #region BlockCommand
        DelegateCommand _BlockCommand;

        public DelegateCommand BlockCommand
        {
            get
            {
                if (_BlockCommand == null)
                    _BlockCommand = new DelegateCommand(Block);
                return _BlockCommand;
            }
        }

        private void Block()
        {
            State = FollowState.Blocking;
        }
        #endregion


        #region UnblockCommand
        DelegateCommand _UnblockCommand;

        public DelegateCommand UnblockCommand
        {
            get
            {
                if (_UnblockCommand == null)
                    _UnblockCommand = new DelegateCommand(Unblock);
                return _UnblockCommand;
            }
        }

        private void Unblock()
        {
            State = FollowState.Unfollowing;
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
            State = FollowState.ReportForSpam;
        }
        #endregion


        #region RefreshInfoCommand
        DelegateCommand _RefreshInfoCommand;

        public DelegateCommand RefreshInfoCommand
        {
            get
            {
                if (_RefreshInfoCommand == null)
                    _RefreshInfoCommand = new DelegateCommand(RefreshInfo);
                return _RefreshInfoCommand;
            }
        }

        private void RefreshInfo()
        {
            IsStandby = false;
            Task.Factory.StartNew(() =>
            {
                UserInformationManager.ReceiveInidividualInfo(Info);
                DispatcherHelper.BeginInvoke(()=>
                {
                    IsStandby = true;
                    RefreshUserData();
                });
            }, TaskCreationOptions.LongRunning);
        }

        private void RefreshUserData()
        {
            if(Info.IsFollowing(TargetUser.TwitterUser.NumericId))
                this.State = FollowState.Following;
            else if (Info.IsBlocking(TargetUser.TwitterUser.NumericId))
                this.State = FollowState.Blocking;
            else
                this.State = FollowState.Unfollowing;
            this.OrigState = State;
            RaisePropertyChanged(() => IsFollowbacked);
        }

        #endregion
    }
}