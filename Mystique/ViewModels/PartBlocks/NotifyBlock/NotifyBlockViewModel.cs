using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Core;
using Inscribe.Storage;
using Octave.Caching;
using Livet.Command;

namespace Mystique.ViewModels.PartBlocks.NotifyBlock
{
    /// <summary>
    /// ステータスバー ViewModel
    /// </summary>
    /// <remarks>
    /// スタティッククラスのイベントリスニングを行いますが、このViewModelのライフサイクルは
    /// アプリケーションライフサイクルと一致するため、メモリリークの問題を回避できます。
    /// </remarks>
    public class NotifyBlockViewModel : ViewModel
    {
        public NotifyBlockViewModel()
        {
            // InformServer.EventRegistered += new InformServer.NotifyEventHandler(InformServer_EventRegistered);
            // InformServer.ReportRegistered += new InformServer.ReportRegisterHandler(InformServer_ReportRegistered);
            ViewModelHelper.BindNotification(NotifyStorage.NotifyTextChangedEvent, this, (o, e) => RaisePropertyChanged(() => StatusText));
            ViewModelHelper.BindNotification(Statistics.WakeupTimeUpdatedEvent, this, (o, e) => RaisePropertyChanged(() => WakeupTime));
            ViewModelHelper.BindNotification(Statistics.TweetSpeedUpdatedEvent, this, (o, e) =>
            {
                RaisePropertyChanged(() => TweetSpeed);
                RaisePropertyChanged(() => TweetSpeedRank);
            });
            ViewModelHelper.BindNotification(TweetStorage.Notificator, this, (o, e) => RaisePropertyChanged(() => Tweets));
            ImageCacheStorage.DownloadingChanged += () => RaisePropertyChanged(() => ImageDownloading);
        }
        
        public enum StateImages
        {
            Ok,
            Info,
            Error
        }

        public StateImages State
        {
            get
            {
                return StateImages.Ok;
            }
        }

        public string StatusText
        {
            get
            {
                return NotifyStorage.Message;
            }
        }

        public int Tweets
        {
            get { return TweetStorage.Count(); }
        }

        public string WakeupTime
        {
            get { return Statistics.WakeupTime; }
        }

        public int TweetSpeed
        {
            get { return Statistics.TweetSpeedPerMin; }
        }

        public enum SpeedRank
        {
            Walk,
            Bicycle,
            Car,
            BulletTrain,
            Maglev,
            Jet,
            Shuttle
        }

        public SpeedRank TweetSpeedRank
        {
            get
            {
                if (TweetSpeed < 20)
                    return SpeedRank.Walk;
                else if (TweetSpeed < 50)
                    return SpeedRank.Bicycle;
                else if (TweetSpeed < 150)
                    return SpeedRank.Car;
                else if (TweetSpeed < 350)
                    return SpeedRank.BulletTrain;
                else if (TweetSpeed < 700)
                    return SpeedRank.Maglev;
                else if (TweetSpeed < 1200)
                    return SpeedRank.Jet;
                else
                    return SpeedRank.Shuttle;
            }
        }

        public bool ImageDownloading
        {
            get { return ImageCacheStorage.Downloading; }
        }

        private bool _isAdditonalInfoOpen = false;
        public bool IsAdditionalInfoOpen
        {
            get { return this._isAdditonalInfoOpen; }
            set
            {
                this._isAdditonalInfoOpen = value;
                RaisePropertyChanged(() => IsAdditionalInfoOpen);
            }
        }

        private SystemInfoViewModel _notifyBlockAdditionalInfoViewModel = new SystemInfoViewModel();
        public SystemInfoViewModel NotifyBlockAdditionalInfoViewModel
        {
            get { return this._notifyBlockAdditionalInfoViewModel; }
        }

        #region ShowAdditionalInfoCommand

        DelegateCommand _ShowAdditionalInfoCommand;

        public DelegateCommand ShowAdditionalInfoCommand
        {
            get
            {
                if (_ShowAdditionalInfoCommand == null)
                    _ShowAdditionalInfoCommand = new DelegateCommand(ShowAdditionalInfo);
                return _ShowAdditionalInfoCommand;
            }
        }

        private void ShowAdditionalInfo()
        {
            this.IsAdditionalInfoOpen = true;
        }

        #endregion

        private bool _isTwitterEventInfoOpen = false;
        public bool IsTwitterEventInfoOpen
        {
            get { return this._isTwitterEventInfoOpen; }
            set
            {
                this._isTwitterEventInfoOpen = value;
                RaisePropertyChanged(() => IsTwitterEventInfoOpen);
            }
        }

        private TwitterEventInfoViewModel _twitterEventInfoViewModel = new TwitterEventInfoViewModel();
        public TwitterEventInfoViewModel TwitterEventInfoViewModel
        {
            get { return this._twitterEventInfoViewModel; }
        }

        #region ShowTwitterEventInfoCommand
        
        DelegateCommand _ShowTwitterEventInfoCommand;

        public DelegateCommand ShowTwitterEventInfoCommand
        {
            get
            {
                if (_ShowTwitterEventInfoCommand == null)
                    _ShowTwitterEventInfoCommand = new DelegateCommand(ShowTwitterEventInfo);
                return _ShowTwitterEventInfoCommand;
            }
        }

        private void ShowTwitterEventInfo()
        {
            this.IsTwitterEventInfoOpen = true;
        }

        #endregion
      
    }
}
