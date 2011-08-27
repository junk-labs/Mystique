using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class TweetViewConfigViewModel : ViewModel, IApplyable
    {
        public bool PowerUserMode
        {
            get { return Setting.Instance.ExperienceProperty.IsPowerUserMode; }
        }

        public TweetViewConfigViewModel()
        {
            this._resolveStrategyIndex = (int)Setting.Instance.TweetExperienceProperty.UrlResolveMode;
            this._tipHelpShowLength = Setting.Instance.TweetExperienceProperty.UrlTooltipShowLength.ToString();
            this._userNameViewModeIndex = (int)Setting.Instance.TweetExperienceProperty.UserNameViewMode;
            this._notificationNameViewModeIndex = (int)Setting.Instance.TweetExperienceProperty.NotificationNameViewMode;
            this._userNameAreaWidthString = Setting.Instance.TweetExperienceProperty.NameAreaWidth.ToString();
            this._p3StyleIcon = Setting.Instance.TweetExperienceProperty.UseP3StyleIcon;
            this._showUnofficialRTButton = Setting.Instance.TweetExperienceProperty.ShowUnofficialRetweetButton;
            this._showQuoteTweetButton = Setting.Instance.TweetExperienceProperty.ShowQuoteButton;
            this._fullLineView = Setting.Instance.TweetExperienceProperty.UseFullLineView;
        }

        private int _resolveStrategyIndex;
        public int ResolveStrategyIndex
        {
            get { return this._resolveStrategyIndex; }
            set
            {
                this._resolveStrategyIndex = value;
                RaisePropertyChanged(() => ResolveStrategyIndex);
            }
        }

        private string _tipHelpShowLength;
        public string TipHelpShowLength
        {
            get { return _tipHelpShowLength; }
            set
            {
                _tipHelpShowLength = value;
                RaisePropertyChanged(() => TipHelpShowLength);
            }
        }

        private int _userNameViewModeIndex;
        public int UserNameViewModeIndex
        {
            get { return this._userNameViewModeIndex; }
            set
            {
                this._userNameViewModeIndex = value;
                RaisePropertyChanged(() => UserNameViewModeIndex);
            }
        }

        private int _notificationNameViewModeIndex;
        public int NotificationNameViewModeIndex
        {
            get { return _notificationNameViewModeIndex; }
            set
            {
                _notificationNameViewModeIndex = value;
                RaisePropertyChanged(() => NotificationNameViewModeIndex);
            }
        }

        private string _userNameAreaWidthString;
        public string UserNameAreaWidthString
        {
            get { return this._userNameAreaWidthString; }
            set
            {
                this._userNameAreaWidthString = value;
                RaisePropertyChanged(() => UserNameAreaWidthString);
                RaisePropertyChanged(() => UserNameAreaWidthInt);
            }
        }

        public int UserNameAreaWidthInt
        {
            get { return int.Parse(_userNameAreaWidthString); }
        }

        private bool _p3StyleIcon;
        public bool P3StyleIcon
        {
            get { return this._p3StyleIcon; }
            set
            {
                this._p3StyleIcon = value;
                RaisePropertyChanged(() => P3StyleIcon);
            }
        }

        private bool _showUnofficialRTButton;
        public bool ShowUnofficialRTButton
        {
            get { return this._showUnofficialRTButton; }
            set
            {
                this._showUnofficialRTButton = value;
                RaisePropertyChanged(() => ShowUnofficialRTButton);
            }
        }

        private bool _showQuoteTweetButton;
        public bool ShowQuoteTweetButton
        {
            get { return this._showQuoteTweetButton; }
            set
            {
                this._showQuoteTweetButton = value;
                RaisePropertyChanged(() => ShowQuoteTweetButton);
            }
        }

        private bool _fullLineView;
        public bool FullLineView
        {
            get { return _fullLineView; }
            set
            {
                _fullLineView = value;
                RaisePropertyChanged(() => FullLineView);
            }
        }

        public void Apply()
        {
            Setting.Instance.TweetExperienceProperty.UrlResolveMode= (UrlResolve)this._resolveStrategyIndex;
            Setting.Instance.TweetExperienceProperty.UrlTooltipShowLength = int.Parse(this._tipHelpShowLength);
            Setting.Instance.TweetExperienceProperty.UserNameViewMode = (NameView)this._userNameViewModeIndex;
            Setting.Instance.TweetExperienceProperty.NotificationNameViewMode = (NameView)this._notificationNameViewModeIndex;
            Setting.Instance.TweetExperienceProperty.NameAreaWidth = this.UserNameAreaWidthInt;
            Setting.Instance.TweetExperienceProperty.UseP3StyleIcon = this._p3StyleIcon;
            Setting.Instance.TweetExperienceProperty.ShowUnofficialRetweetButton = this._showUnofficialRTButton;
            Setting.Instance.TweetExperienceProperty.ShowQuoteButton = this._showQuoteTweetButton;
            Setting.Instance.TweetExperienceProperty.UseFullLineView = this._fullLineView;
        }
    }
}
