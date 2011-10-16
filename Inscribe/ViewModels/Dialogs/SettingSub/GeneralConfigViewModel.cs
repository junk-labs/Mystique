using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Media;
using Inscribe.Configuration;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class GeneralConfigViewModel : ViewModel, IApplyable
    {
        public GeneralConfigViewModel()
        {
            this._powerUserMode = Setting.Instance.ExperienceProperty.IsPowerUserMode;
            this._updateKind = Setting.Instance.ExperienceProperty.UpdateKind;
            this.FontFamilies = Fonts.SystemFontFamilies.ToArray();
            var curFF = String.IsNullOrEmpty(Setting.Instance.ExperienceProperty.FontFamily) ?
                new FontFamily() :
                new FontFamily(Setting.Instance.ExperienceProperty.FontFamily);
            this._fontFamilyIndex = Array.IndexOf(this.FontFamilies.ToArray(), curFF);
            this.FontSize = Setting.Instance.ExperienceProperty.FontSize;
            this.IgnoreTimeoutError = Setting.Instance.ExperienceProperty.IgnoreTimeoutError;
        }

        private bool _powerUserMode;
        public bool PowerUserMode
        {
            get { return _powerUserMode; }
            set
            {
                _powerUserMode = value;
                RaisePropertyChanged(() => PowerUserMode);
            }
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
                return FontFamilies.Select(ff => ff.FamilyNames.ContainsKey(jaJP) ?
                    ff.FamilyNames[jaJP] : ff.FamilyNames.Select(xl => xl.Value).FirstOrDefault());
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
            Setting.Instance.ExperienceProperty.IsPowerUserMode = this._powerUserMode;
            Setting.Instance.ExperienceProperty.UpdateKind = this._updateKind;
            Setting.Instance.ExperienceProperty.FontFamily =
                FontFamilies.Select(ff => ff.FamilyNames.Select(fn => fn.Value).FirstOrDefault())
                .ElementAtOrDefault(_fontFamilyIndex);
            Setting.Instance.ExperienceProperty.FontSize = this.FontSize;
            Setting.Instance.ExperienceProperty.IgnoreTimeoutError = this.IgnoreTimeoutError;
        }
    }
}
