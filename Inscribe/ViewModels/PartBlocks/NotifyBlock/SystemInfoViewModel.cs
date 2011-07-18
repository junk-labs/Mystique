using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Model;
using Inscribe.Configuration;
using System.Threading;
using Inscribe.Threading;
using Inscribe.Communication.Posting;
using Inscribe.Storage;
using System.Windows.Media;
using Livet.Commands;
using System.Threading.Tasks;
using Inscribe.ViewModels.Common;

namespace Inscribe.ViewModels.PartBlocks.NotifyBlock
{
    public class SystemInfoViewModel : ViewModel
    {
        public SystemInfoViewModel()
        {
            ViewModelHelper.BindNotification(AccountStorage.AccountsChangedEvent, this, (o, e) => RaisePropertyChanged(() => Accounts));
            ViewModelHelper.BindNotification(ExceptionStorage.ExceptionUpdatedEvent, this, (o, e) => RaisePropertyChanged(() => Exceptions));
        }

        public IEnumerable<AccountViewModel> Accounts
        {
            get
            {
                return AccountStorage.Accounts.Select(i => new AccountViewModel(i));
            }
        }

        public IEnumerable<ExceptionViewModel> Exceptions
        {
            get
            {
                return ExceptionStorage.Exceptions.Select(e => new ExceptionViewModel(e));
            }
        }

        #region ClearAllExceptionsCommand
        DelegateCommand _ClearAllExceptionsCommand;

        public DelegateCommand ClearAllExceptionsCommand
        {
            get
            {
                if (_ClearAllExceptionsCommand == null)
                    _ClearAllExceptionsCommand = new DelegateCommand(ClearAllExceptions);
                return _ClearAllExceptionsCommand;
            }
        }

        private void ClearAllExceptions()
        {
            foreach (var e in ExceptionStorage.Exceptions.ToArray())
            {
                ExceptionStorage.Remove(e);
            }
        }
        #endregion
      
    }

    public class AccountViewModel : ViewModel
    {
        static Notificator<EventArgs> TimeTickCall = new Notificator<EventArgs>();

        static Timer tickCall = null;

        static AccountViewModel()
        {
            tickCall = new Timer(
                _ => TimeTickCall.Raise(EventArgs.Empty),
                null, 0, 20 * 1000);
            ThreadHelper.Halt += () => tickCall.Dispose();
        }

        private readonly AccountInfo Info;

        public AccountViewModel(AccountInfo info)
        {
            
            this.Info = info;
            this._profileImageProvider = new Common.ProfileImageProvider(info);
            ViewModelHelper.BindNotification(info.UserStreamsConnectionChangedEvent, this, (o, e) => RaisePropertyChanged(() => ConnectState));
            ViewModelHelper.BindNotification(TimeTickCall, this, (o, e) =>
            {
                RaisePropertyChanged(() => ApiRemain);
                RaisePropertyChanged(() => ApiMax);
                RaisePropertyChanged(() => ApiReset);
            });
            ViewModelHelper.BindNotification(PostOffice.OnUnderControlChangedEvent, this, (o, e) =>
            {
                RaisePropertyChanged(() => IsAccountUnderControlled);
                RaisePropertyChanged(() => AccountControlReleaseTime);
            });
        }

        private ProfileImageProvider _profileImageProvider;
        public ProfileImageProvider ProfileImageProvider
        {
            get { return this._profileImageProvider; }
        }

        public string ScreenName { get { return this.Info.ScreenName; } }

        public string ConnectState
        {
            get
            {
                switch (this.Info.UserStreamsConnectionState)
                {
                    case ConnectionState.Disconnected:
                        if (Setting.Instance.ExperienceProperty.PowerUserMode)
                            return "REST APIで接続しています";
                        else
                            return "接続しています";
                    case ConnectionState.WaitNetwork:
                        if (Setting.Instance.ExperienceProperty.PowerUserMode)
                            return "User Streams: ネットワーク接続を待っています...";
                        else
                            return "リアルタイム接続を試行しています...";
                    case ConnectionState.WaitTwitter:
                        if (Setting.Instance.ExperienceProperty.PowerUserMode)
                            return "User Streams: Twitterの応答を待っています...";
                        else
                            return "リアルタイム接続を試行しています...";
                    case ConnectionState.TryConnection:
                        if (Setting.Instance.ExperienceProperty.PowerUserMode)
                            return "User Streams 接続を開始しています...";
                        else
                            return "リアルタイム接続を開始しています...";
                    case ConnectionState.Connected:
                        if (Setting.Instance.ExperienceProperty.PowerUserMode)
                            return "User Streams 接続しています";
                        else
                            return "リアルタイム接続しています";
                    default:
                        return "接続状況が不明です。";
                }
            }
        }

        public string ApiRemain
        {
            get { return this.Info.RateLimitRemaining.ToString(); }
        }

        public string ApiMax
        {
            get { return this.Info.RateLimitMax.ToString(); }
        }

        public string ApiReset
        {
            get { return this.Info.RateLimitReset.ToLocalTime().ToString(); }
        }

        public bool IsAccountUnderControlled
        {
            get
            {
                return PostOffice.IsAccountUnderControlled(this.Info);
            }
        }

        public string AccountControlReleaseTime
        {
            get
            {
                return PostOffice.GetAccountInfoControlReleaseTime(this.Info).ToLocalTime().ToString();
            }
        }
    }

    public class ExceptionViewModel : ViewModel
    {
        private readonly ExceptionDescription desc;

        public ExceptionViewModel(ExceptionDescription excp)
        {
            this.desc = excp;
        }

        public Brush Foreground
        {
            get
            {
                switch (desc.Category)
                {
                    case ExceptionCategory.AssertionFailed:
                        return Brushes.Gray;
                    case ExceptionCategory.ConfigurationError:
                        return Brushes.Orange;
                    case ExceptionCategory.InternalError:
                    case ExceptionCategory.PluginError:
                        return Brushes.Red;
                    case ExceptionCategory.TwitterError:
                        return Brushes.Navy;
                    case ExceptionCategory.UserError:
                        return Brushes.Orange;
                    default:
                        return Brushes.Black;
                }
            }
        }

        public string Kind
        {
            get
            {
                switch (desc.Category)
                {
                    case ExceptionCategory.AssertionFailed:
                        return "予備情報";
                    case ExceptionCategory.ConfigurationError:
                        return "設定エラー";
                    case ExceptionCategory.InternalError:
                        return "内部エラー";
                    case ExceptionCategory.PluginError:
                        return "プラグインエラー";
                    case ExceptionCategory.TwitterError:
                        return "Twitter通信エラー";
                    case ExceptionCategory.UserError:
                        return "ユーザー操作エラー";
                    default:
                        return "不明なエラー";
                }
            }
        }

        public string Body
        {
            get
            {
                if (String.IsNullOrEmpty(desc.Message))
                    return desc.Exception.Message;
                else
                    return desc.Message + " - " + desc.Exception.Message;
            }
        }


        #region RetryCommand

        DelegateCommand _RetryCommand;

        public DelegateCommand RetryCommand
        {
            get
            {
                if (_RetryCommand == null)
                    _RetryCommand = new DelegateCommand(Retry, () => desc.RetryAction != null);
                return _RetryCommand;
            }
        }

        private void Retry()
        {
            ExceptionStorage.Remove(desc);
            Task.Factory.StartNew(() => desc.RetryAction());
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
            ExceptionStorage.Remove(desc);
        }
        
        #endregion
      
    }
}
