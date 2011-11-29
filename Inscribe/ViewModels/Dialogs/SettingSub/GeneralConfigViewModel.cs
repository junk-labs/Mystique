using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Media;
using Inscribe.Configuration;
using Livet;
using Livet.Messaging;
using Inscribe.Storage;
using Inscribe.Communication.Posting;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class GeneralConfigViewModel : ViewModel, IApplyable
    {
        private bool fontBroken = false;
        public GeneralConfigViewModel()
        {
            this._aloofUserMode = Setting.Instance.ExperienceProperty.IsTranscender;
            this._updateKind = Setting.Instance.ExperienceProperty.UpdateKind + 1;
            this.FontFamilies = Fonts.SystemFontFamilies.ToArray();
            var curFF = String.IsNullOrEmpty(Setting.Instance.ExperienceProperty.FontFamily) ?
                new FontFamily() :
                new FontFamily(Setting.Instance.ExperienceProperty.FontFamily);
            this._fontFamilyIndex = Array.IndexOf(this.FontFamilies.ToArray(), curFF);
            this.FontSize = Setting.Instance.ExperienceProperty.FontSize;
            this.IgnoreTimeoutError = Setting.Instance.ExperienceProperty.IgnoreTimeoutError;
        }

        private bool _aloofUserMode;
        public bool IsTranscender
        {
            get { return _aloofUserMode; }
            set
            {
                _aloofUserMode = value;
                if (value)
                {
                    if (AccountStorage.Accounts.Count() == 0)
                    {
                        _aloofUserMode = false;
                    }
                    else
                    {
                        var cm = new ConfirmationMessage(
                            "もうもどれませんがよろしいですか？",
                            "また新たな超越者が生まれた...",
                             System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxButton.OKCancel, "Confirm");
                        this.Messenger.Raise(cm);
                        if (!cm.Response.GetValueOrDefault())
                        {
                            _aloofUserMode = false;
                        }
                        else
                        {
                            var a = AccountStorage.Accounts.FirstOrDefault();
                            PostOffice.UpdateTweet(a, "私は超越しました。http://krile.starwing.net/ #krile #超越しました");
                            Setting.Instance.ExperienceProperty.IsTranscender = true;
                        }
                    }
                }
                RaisePropertyChanged(() => IsTranscender);
                RaisePropertyChanged(() => IsPowerUserEnableCheckEnabled);
            }
        }

        public bool IsPowerUserEnableCheckEnabled
        {
            get { return !IsTranscender && AccountStorage.Accounts.Count() > 0; }
        }

        private int _updateKind;
        public int UpdateKind
        {
            get { return _updateKind; }
            set
            {
                _updateKind = value;
                RaisePropertyChanged(() => UpdateKind);
            }
        }

        public IEnumerable<FontFamily> FontFamilies { get; set; }

        public IEnumerable<String> DisplayFontFamilies
        {
            get
            {
                var jaJP = XmlLanguage.GetLanguage("ja-JP");
                try
                {
                    return FontFamilies.Select(ff => ff.FamilyNames.ContainsKey(jaJP) ?
                        ff.FamilyNames[jaJP] : ff.FamilyNames.Select(xl => xl.Value).FirstOrDefault())
                        .ToArray();
                }
                catch (ArgumentException)
                {
                    fontBroken = true;
                    FontFamilyIndex = -1;
                    return new[] { "フォント情報が破損しています。フォント設定は利用できません。" };
                }
            }
        }

        private int _fontFamilyIndex;
        public int FontFamilyIndex
        {
            get { return _fontFamilyIndex; }
            set
            {
                _fontFamilyIndex = value;
                RaisePropertyChanged(() => FontFamilyIndex);
            }
        }

        private double _fontSize;
        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                RaisePropertyChanged(() => FontSize);
            }
        }

        private bool _ignoreTimeoutError;
        public bool IgnoreTimeoutError
        {
            get { return _ignoreTimeoutError; }
            set
            {
                _ignoreTimeoutError = value;
                RaisePropertyChanged(() => IgnoreTimeoutError);
            }
        }

        public void Apply()
        {
            Setting.Instance.ExperienceProperty.UpdateKind = this._updateKind - 1;
            if (!fontBroken)
            {
                Setting.Instance.ExperienceProperty.FontFamily =
                    FontFamilies.Select(ff => ff.FamilyNames.Select(fn => fn.Value).FirstOrDefault())
                    .ElementAtOrDefault(_fontFamilyIndex);
            }
            Setting.Instance.ExperienceProperty.FontSize = this.FontSize;
            Setting.Instance.ExperienceProperty.IgnoreTimeoutError = this.IgnoreTimeoutError;
        }
    }
}
