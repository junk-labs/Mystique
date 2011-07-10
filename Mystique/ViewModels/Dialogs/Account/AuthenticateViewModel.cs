using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Inscribe.Common;
using Inscribe.Communication;
using Inscribe.Configuration;
using Inscribe.Model;
using Livet;
using Livet.Command;
using Livet.Messaging;
using Livet.Messaging.Window;

namespace Mystique.ViewModels.Dialogs.Account
{
    public class AuthenticateViewModel : ViewModel
    {
        private CredentialCore _credentialCore;

        public bool Success { get; private set; }

        public AccountInfo GetAccountInfo(AccountInfo previous = null)
        {
            if (previous != null)
            {
                previous.RewriteCredential(this._credentialCore);
                return previous;
            }
            else
            {
                return AccountInfo.FromCredential(this._credentialCore);
            }
        }

        public AuthenticateViewModel(CredentialCore prevCore = null)
        {
            this.Success = false;
            this._credentialCore = new CredentialCore();
            if (prevCore != null)
            {
                this._credentialCore.OverridedConsumerKey = prevCore.OverridedConsumerKey;
                this._credentialCore.OverridedConsumerSecret = prevCore.OverridedConsumerSecret;
            }
        }

        private bool _isStandby = true;
        public bool IsStandby
        {
            get { return this._isStandby; }
            set
            {
                this._isStandby = value;
                RaisePropertyChanged(() => IsStandby);
            }
        }

        public bool CanOverrideAPIKey
        {
            get { return Setting.Instance.ExperienceProperty.PowerUserMode; }
        }

        public bool IsOverridesConsumerInfo
        {
            get
            {
                return
                    !String.IsNullOrEmpty(this._credentialCore.OverridedConsumerKey) ||
                    !String.IsNullOrEmpty(this._credentialCore.OverridedConsumerSecret);
            }
        }

        public string OverridedConsumerKey
        {
            get { return this._credentialCore.OverridedConsumerKey; }
            set
            {
                this._credentialCore.OverridedConsumerKey = value;
                RaisePropertyChanged(() => OverridedConsumerKey);
                RaisePropertyChanged(() => IsOverridesConsumerInfo);
                RaisePropertyChanged(() => CanOverrideAPIKey);
            }
        }

        public string OverridedConsumerSecret
        {
            get { return this._credentialCore.OverridedConsumerSecret; }
            set
            {
                this._credentialCore.OverridedConsumerSecret = value;
                RaisePropertyChanged(() => OverridedConsumerSecret);
                RaisePropertyChanged(() => IsOverridesConsumerInfo);
                RaisePropertyChanged(() => CanOverrideAPIKey);
            }
        }

        public bool GotToken { get { return this._requestToken != null; } }


        private string _requestToken = null;
        private string requestToken
        {
            get { return _requestToken; }
            set
            {
                this._requestToken = value;
                this.ValidatePinCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(() => GotToken);
            }
        }


        #region BeginAuthorizeCommand
        DelegateCommand _BeginAuthorizeCommand;

        public DelegateCommand BeginAuthorizeCommand
        {
            get
            {
                if (_BeginAuthorizeCommand == null)
                    _BeginAuthorizeCommand = new DelegateCommand(BeginAuthorize);
                return _BeginAuthorizeCommand;
            }
        }

        private void BeginAuthorize()
        {
            this.IsStandby = false;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    string token = null;
                    var uri = ApiHelper.ExecApi(() => this._credentialCore.GetProviderAuthUrl(out token));
                    if (uri == null || String.IsNullOrEmpty(token))
                    {
                        throw new Exception("リターン情報が空です。(uri:" +
                            (uri == null ? "NULL" : uri.OriginalString) + " / token: " + token);

                    }
                    this.requestToken = token;
                    Browser.Start(uri.OriginalString);
                }
                catch (Exception e)
                {
                    DispatcherHelper.BeginInvoke(() =>
                    this.Messenger.Raise(new InformationMessage(
                        "認証を開始できませんでした。" + Environment.NewLine +
                        "もう一度試してください。" + Environment.NewLine +
                        "エラー: " + e.Message,
                        "認証開始エラー", System.Windows.MessageBoxImage.Error, "Inform")));
                }
                finally
                {
                    this.IsStandby = true;
                }
            });
        }
        #endregion

        private string _pinString = String.Empty;
        public string PinString
        {
            get { return _pinString; }
            set
            {
                _pinString = value;
                RaisePropertyChanged(() => PinString);
                ValidatePinCommand.RaiseCanExecuteChanged();
            }
        }

        #region ValidatePinCommand
        DelegateCommand _ValidatePinCommand;

        public DelegateCommand ValidatePinCommand
        {
            get
            {
                if (_ValidatePinCommand == null)
                    _ValidatePinCommand = new DelegateCommand(ValidatePin, CanValidatePin);
                return _ValidatePinCommand;
            }
        }

        private bool CanValidatePin()
        {

            return !String.IsNullOrEmpty(requestToken) && !String.IsNullOrEmpty(PinString);
        }

        private void ValidatePin()
        {
            this.IsStandby = false;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    string userId = null;
                    if (this._credentialCore.GetAccessToken(
                        this.requestToken, this.PinString, out userId) &&
                        !String.IsNullOrEmpty(userId))
                    {
                        this._credentialCore.ScreenName = userId;
                        this.Success = true;
                        DispatcherHelper.BeginInvoke(() =>
                        Messenger.Raise(new WindowActionMessage("Close", WindowAction.Close)));
                    }
                    else
                    {
                        requestToken = String.Empty;
                        PinString = String.Empty;
                        throw new Exception("認証に失敗しました。PINの入力ミスの可能性があります。");
                    }
                }
                catch (WebException wex)
                {
                    string errormsg = string.Empty;
                    if (wex.Response != null)
                    {
                        using (var sr = new StreamReader(wex.Response.GetResponseStream()))
                        {
                            errormsg = sr.ReadToEnd();
                        }
                    }
                    else
                    {
                        errormsg = "データはありません。";
                    }
                    DispatcherHelper.BeginInvoke(() =>
                    Messenger.Raise(new InformationMessage(
                            "認証に失敗しました。もう一度やり直してください。" + Environment.NewLine +
                                "エラーデータ:" + errormsg + Environment.NewLine +
                                wex.Message, "認証エラー", System.Windows.MessageBoxImage.Error, "Inform")));
                }
                catch (Exception ex)
                {
                    DispatcherHelper.BeginInvoke(() =>
                    Messenger.Raise(new InformationMessage(
                            "認証に失敗しました。もう一度やり直してください。" + Environment.NewLine +
                                ex.Message, "認証エラー", System.Windows.MessageBoxImage.Error, "Inform")));
                }
                finally
                {
                    this.IsStandby = true;
                }
            });
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
            DispatcherHelper.BeginInvoke(() =>
                Messenger.Raise(new WindowActionMessage("Close", WindowAction.Close)));
        }

        #endregion

        #region NavigateOAuthLogoCommand
        DelegateCommand _NavigateOAuthLogoCommand;

        public DelegateCommand NavigateOAuthLogoCommand
        {
            get
            {
                if (_NavigateOAuthLogoCommand == null)
                    _NavigateOAuthLogoCommand = new DelegateCommand(NavigateOAuthLogo);
                return _NavigateOAuthLogoCommand;
            }
        }

        private void NavigateOAuthLogo()
        {
            Browser.Start("http://factoryjoe.com/");
        }
        #endregion
    }
}
