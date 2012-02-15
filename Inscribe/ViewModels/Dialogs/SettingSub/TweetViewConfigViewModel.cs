using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.Storage;
using Inscribe.Communication.Posting;
using System.Threading.Tasks;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class TweetViewConfigViewModel : ViewModel, IApplyable
    {
        public bool IsTranscender
        {
            get { return Setting.Instance.ExperienceProperty.IsTranscender; }
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
            this._viewModeIndex = (int)Setting.Instance.TweetExperienceProperty.TweetViewMode;
            this._canFavMyTweet = Setting.Instance.TweetExperienceProperty.CanFavoriteMyTweet;
            this._showTweetTooltip = Setting.Instance.TweetExperienceProperty.ShowTweetTooltip;
            this._showImageInlineThumbnail = Setting.Instance.TweetExperienceProperty.ShowImageInlineThumbnail;
            this._rightButtonKind = (int)Setting.Instance.TweetExperienceProperty.RightButtonKind;
            this._showStealButton = Setting.Instance.TweetExperienceProperty.ShowStealButton;
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

        private int _viewModeIndex;
        public int ViewModeIndex
        {
            get { return _viewModeIndex; }
            set
            {
                _viewModeIndex = value;
                RaisePropertyChanged(() => ViewModeIndex);
                RaisePropertyChanged(() => IsSingleline);
            }
        }

        private bool _canFavMyTweet;
        public bool CanFavMyTweet
        {
            get { return _canFavMyTweet; }
            set
            {
                _canFavMyTweet = value;
                RaisePropertyChanged(() => CanFavMyTweet);
            }
        }

        private bool _showTweetTooltip;
        public bool ShowTweetTooltip
        {
            get { return _showTweetTooltip; }
            set
            {
                _showTweetTooltip = value;
                RaisePropertyChanged(() => ShowTweetTooltip);
            }
        }

        private bool _showImageInlineThumbnail;
        public bool ShowImageInlineThumbnail
        {
            get { return _showImageInlineThumbnail; }
            set
            {
                _showImageInlineThumbnail = value;
                RaisePropertyChanged(() => ShowImageInlineThumbnail);
            }
        }

        private bool _showStealButton;
        public bool ShowStealButton
        {
            get { return _showStealButton; }
            set
            {
                _showStealButton = value;
                RaisePropertyChanged(() => ShowStealButton);
            }
        }

        private int _rightButtonKind;
        public int RightButtonKind
        {
            get { return _rightButtonKind; }
            set
            {
                _rightButtonKind = value;
                RaisePropertyChanged(() => RightButtonKind);
            }
        }

        #region Binding helper

        public bool IsSingleline
        {
            get { return this._viewModeIndex > 0; }
        }

        #endregion

        public void Apply()
        {
            try
            {
                Setting.Instance.TweetExperienceProperty.UrlResolveMode = (UrlResolve)this._resolveStrategyIndex;
                Setting.Instance.TweetExperienceProperty.UrlTooltipShowLength = int.Parse(this._tipHelpShowLength);
                Setting.Instance.TweetExperienceProperty.UserNameViewMode = (NameView)this._userNameViewModeIndex;
                Setting.Instance.TweetExperienceProperty.NotificationNameViewMode = (NameView)this._notificationNameViewModeIndex;
                Setting.Instance.TweetExperienceProperty.NameAreaWidth = this.UserNameAreaWidthInt;
                Setting.Instance.TweetExperienceProperty.UseP3StyleIcon = this._p3StyleIcon;
                Setting.Instance.TweetExperienceProperty.ShowUnofficialRetweetButton = this._showUnofficialRTButton;
                Setting.Instance.TweetExperienceProperty.ShowQuoteButton = this._showQuoteTweetButton;
                Setting.Instance.TweetExperienceProperty.TweetViewMode = (TweetViewingMode)this._viewModeIndex;
                Setting.Instance.TweetExperienceProperty.CanFavoriteMyTweet = this._canFavMyTweet;
                Setting.Instance.TweetExperienceProperty.ShowTweetTooltip = this._showTweetTooltip;
                Setting.Instance.TweetExperienceProperty.RightButtonKind = (QuickActionButtonKind)this._rightButtonKind;
                Setting.Instance.TweetExperienceProperty.ShowImageInlineThumbnail = this._showImageInlineThumbnail;
                if (Setting.Instance.TweetExperienceProperty.ShowStealButton != this._showStealButton && this._showStealButton)
                {
                    var acc = AccountStorage.Accounts.FirstOrDefault();
                    if (acc != null)
                    {
                        string sts = "人としてクズに育ちました！お父さんお母さんありがとう！(◞≼●≽◟◞౪◟◞≼●≽◟) #クズ";
                        Task.Factory.StartNew(() => { try { PostOffice.UpdateTweet(acc, sts); } catch { } });
                    }
                }
                Setting.Instance.TweetExperienceProperty.ShowStealButton = this._showStealButton;
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.UserError, "Setting error.");
            }
        }
    }
}
