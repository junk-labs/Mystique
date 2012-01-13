using System.Collections.Generic;
using System.Linq;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.ViewModels.Common;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Inscribe.Common;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class ColoringConfigViewModel : ViewModel, IApplyable
    {
        public IEnumerable<IApplyable> NameBackColors { get; private set; }
        public IEnumerable<IApplyable> TextBackColors { get; private set; }
        public IEnumerable<IApplyable> TextForeColors { get; private set; }
        public IEnumerable<IApplyable> CommonColors { get; private set; }

        public bool SetInputCaretColorWhite { get; set; }

        public bool SetSearchCaretColorWhite { get; set; }

        private int _coloringIndex;
        public int ColoringIndex
        {
            get { return _coloringIndex; }
            set
            {
                _coloringIndex = value;
                RaisePropertyChanged(() => ColoringIndex);
            }
        }

        private string _backgroundImage;
        public string BackgroundImage
        {
            get { return _backgroundImage; }
            set
            {
                _backgroundImage = value;
                RaisePropertyChanged(() => BackgroundImage);
            }
        }

        public ColoringConfigViewModel()
        {
            SetInputCaretColorWhite = Setting.Instance.ColoringProperty.SetInputCaretColorWhite;
            SetSearchCaretColorWhite = Setting.Instance.ColoringProperty.SetSearchCaretColorWhite;
            this._coloringIndex = (int)Setting.Instance.ColoringProperty.TweetColorMode;
            NameBackColors = new IApplyable[]{
                Wrap(Setting.Instance.ColoringProperty.DefaultHighlightColor, "基本色"),
                Wrap(Setting.Instance.ColoringProperty.MyColor, "自分"),
                Wrap(Setting.Instance.ColoringProperty.FriendColor, "相互フォローユーザー"),
                Wrap(Setting.Instance.ColoringProperty.FollowingColor, "片思いユーザー"),
                Wrap(Setting.Instance.ColoringProperty.FollowerColor, "片思われユーザー"),
                Wrap(Setting.Instance.ColoringProperty.DirectMessageHighlightColor, "ダイレクトメッセージ"),
            };
            TextBackColors = new IApplyable[]{
                Wrap(Setting.Instance.ColoringProperty.DefaultColor, "基本色"),
                Wrap(Setting.Instance.ColoringProperty.RetweetedColor, "リツイート"),
                Wrap(Setting.Instance.ColoringProperty.MentionColor, "@mention"),
                Wrap(Setting.Instance.ColoringProperty.SelectedColor, "選択中のツイートと同じユーザー"),
                Wrap(Setting.Instance.ColoringProperty.DirectMessageColor, "ダイレクトメッセージ"),
            };
            TextForeColors = new IApplyable[]{
                Wrap(Setting.Instance.ColoringProperty.DefaultTextColor, "基本色"),
                Wrap(Setting.Instance.ColoringProperty.DefaultLinkColor, "リンク文字色"),
                Wrap(Setting.Instance.ColoringProperty.RetweetedTextColor, "リツイート"),
                Wrap(Setting.Instance.ColoringProperty.MentionTextColor, "@mention"),
                Wrap(Setting.Instance.ColoringProperty.SelectedTextColor, "選択中のツイートと同じユーザー"),
                Wrap(Setting.Instance.ColoringProperty.DirectMessageTextColor, "ダイレクトメッセージ"),
            };
            CommonColors = new IApplyable[]{
                Wrap(Setting.Instance.ColoringProperty.PostBoxOpenForeground, "投稿欄文字"),
                Wrap(Setting.Instance.ColoringProperty.PostBoxOpenBackground, "投稿欄背景"),
                Wrap(Setting.Instance.ColoringProperty.PostBoxCloseForeground, "投稿欄(閉)文字"),
                Wrap(Setting.Instance.ColoringProperty.PostBoxCloseBackground, "投稿欄(閉)背景"),
                Wrap(Setting.Instance.ColoringProperty.PostBoxBorder, "投稿欄枠線"),
                Wrap(Setting.Instance.ColoringProperty.SearchBackground, "検索バー全体背景"),
                Wrap(Setting.Instance.ColoringProperty.SearchForeground, "検索バー文字"),
                Wrap(Setting.Instance.ColoringProperty.SearchInactiveForeground, "検索バーウォーターマーク"),
                Wrap(Setting.Instance.ColoringProperty.SearchTextBackground, "検索バー入力背景"),
                Wrap(Setting.Instance.ColoringProperty.SearchBorder, "検索バー枠線"),
                Wrap(Setting.Instance.ColoringProperty.StatusBarBackground, "ステータスバー背景"),
                Wrap(Setting.Instance.ColoringProperty.TweetWorkerNotifierBackground, "投稿進捗/通知背景"),
                Wrap(Setting.Instance.ColoringProperty.TabBackground, "タブ背景"),
                Wrap(Setting.Instance.ColoringProperty.TabHighlight, "タブハイライト"),
                Wrap(Setting.Instance.ColoringProperty.TabSelectedBackground, "選択タブ背景"),
                Wrap(Setting.Instance.ColoringProperty.TabSelectedHighlight, "選択タブハイライト"),
                Wrap(Setting.Instance.ColoringProperty.UserProfileBackground, "ユーザープロフィール背景"),
                Wrap(Setting.Instance.ColoringProperty.UserProfileDarkBackground, "ユーザープロフィール背景(暗)"),
            };
            this.BackgroundImage = Setting.Instance.TimelineExperienceProperty.BackgroundImage;
        }

        private IApplyable Wrap(ColorElement elem, string desc)
        {
            return new ColorElementViewModel(desc, elem);
        }

        private IApplyable Wrap(DisablableColorElement elem, string desc)
        {
            return new DisablableColorElementViewModel(desc, elem);
        }

        private IApplyable Wrap(PairColorElement elem, string desc)
        {
            return new PairColorElementViewModel(desc, elem);
        }

        private IApplyable Wrap(DisablablePairColorElement elem, string desc)
        {
            return new DisablablePairColorElementViewModel(desc, elem);
        }

        #region OpenFileDialogCommand
        private ViewModelCommand _OpenFileDialogCommand;

        public ViewModelCommand OpenFileDialogCommand
        {
            get
            {
                if (_OpenFileDialogCommand == null)
                {
                    _OpenFileDialogCommand = new ViewModelCommand(OpenFileDialog);
                }
                return _OpenFileDialogCommand;
            }
        }

        public void OpenFileDialog()
        {
            var msg = new OpeningFileSelectionMessage("OpenFile");
            msg.Filter = "画像ファイル|*.jpg;*.jpeg;*.jpe;*.gif;*.png;*.bmp";
            msg.Title = "背景画像を選択";
            string resp;
            if ((resp = this.Messenger.GetResponse(msg).Response) != null)
            {
                this.BackgroundImage = resp;
            }
        }
        #endregion

        #region SetDefaultColorCommand
        private ListenerCommand<ConfirmationMessage> _SetDefaultColorCommand;

        public ListenerCommand<ConfirmationMessage> SetDefaultColorCommand
        {
            get
            {
                if (_SetDefaultColorCommand == null)
                {
                    _SetDefaultColorCommand = new ListenerCommand<ConfirmationMessage>(SetDefaultColor);
                }
                return _SetDefaultColorCommand;
            }
        }

        public void SetDefaultColor(ConfirmationMessage parameter)
        {
            if (parameter.Response.GetValueOrDefault())
            {
                // デフォルトカラーを設定
                ApplyColoringProperty(new ColoringProperty());
            }
        }
        #endregion

        public void ApplyColoringProperty(ColoringProperty cp)
        {
            var nbcprops = new dynamic[]
            {
                cp.DefaultHighlightColor, 
                cp.MyColor, 
                cp.FriendColor, 
                cp.FollowingColor, 
                cp.FollowerColor, 
                cp.DirectMessageHighlightColor
            };
            var tbcprops = new dynamic[] 
            { 
                cp.DefaultColor,
                cp.RetweetedColor, 
                cp.MentionColor,
                cp.SelectedColor, 
                cp.DirectMessageColor 
            };
            var tfcprops = new dynamic[] 
            { 
                cp.DefaultTextColor,
                cp.DefaultLinkColor, 
                cp.RetweetedTextColor, 
                cp.MentionTextColor, 
                cp.SelectedTextColor, 
                cp.DirectMessageTextColor 
            };
            var ccprops = new dynamic[] 
            {
                cp.PostBoxOpenForeground,
                cp.PostBoxOpenBackground,
                cp.PostBoxCloseForeground,
                cp.PostBoxCloseBackground,
                cp.PostBoxBorder,
                cp.SearchBackground,
                cp.SearchForeground,
                cp.SearchInactiveForeground,
                cp.SearchTextBackground,
                cp.SearchBorder,
                cp.StatusBarBackground,
                cp.TweetWorkerNotifierBackground,
                cp.TabBackground,
                cp.TabHighlight,
                cp.TabSelectedBackground,
                cp.TabSelectedHighlight,
                cp.UserProfileBackground,
                cp.UserProfileDarkBackground
            };

            SetInputCaretColorWhite = cp.SetInputCaretColorWhite;
            SetSearchCaretColorWhite = cp.SetSearchCaretColorWhite;

            NameBackColors.Concat(TextBackColors).Concat(TextForeColors).Concat(CommonColors)
                .Select(a => (dynamic)a)
                .Zip(nbcprops.Concat(tbcprops).Concat(tfcprops).Concat(ccprops),
                    (a, e) => new { a, e })
                .ForEach(v => RefreshValue(v.a, v.e));
        }

        private void RefreshValue(ColorElementViewModel vm, IColorElement ce)
        {
            vm.CPBViewModel.CurrentColor = ce.GetColor();
        }

        private void RefreshValue(PairColorElementViewModel vm, PairColorElement ce)
        {
            vm.LightViewModel.CPBViewModel.CurrentColor = ce.GetLightColor();
            vm.DarkViewModel.CPBViewModel.CurrentColor = ce.GetDarkColor();
        }

        private void RefreshValue(DisablablePairColorElementViewModel vm, DisablablePairColorElement ce)
        {
            vm.LightViewModel.CPBViewModel.CurrentColor = ce.GetLightColor();
            vm.DarkViewModel.CPBViewModel.CurrentColor = ce.GetDarkColor();
        }

        #region FileImportCommand
        private ViewModelCommand _FileImportCommand;

        public ViewModelCommand FileImportCommand
        {
            get
            {
                if (_FileImportCommand == null)
                {
                    _FileImportCommand = new ViewModelCommand(FileImport);
                }
                return _FileImportCommand;
            }
        }

        public void FileImport()
        {
            var ofm = new OpeningFileSelectionMessage("OpenFile");
            ofm.Title = "色設定ファイルを選択";
            ofm.Filter = "色設定ファイル|*.kcx";
            this.Messenger.Raise(ofm);
            if (ofm.Response != null)
            {
                try
                {
                    var data = XMLSerializer.LoadXML<ColoringProperty>(ofm.Response, true);
                    ApplyColoringProperty(data);
                }
                catch
                {
                    this.Messenger.Raise(new InformationMessage("ファイルを読み込めません。",
                        "色設定ファイルのロードエラー", System.Windows.MessageBoxImage.Error, "Message"));
                }
            }
        }
        #endregion

        #region FileExportCommand
        private ViewModelCommand _FileExportCommand;

        public ViewModelCommand FileExportCommand
        {
            get
            {
                if (_FileExportCommand == null)
                {
                    _FileExportCommand = new ViewModelCommand(FileExport);
                }
                return _FileExportCommand;
            }
        }

        public void FileExport()
        {
            var cm = new ConfirmationMessage(
                "色設定をエクスポートすると、現在の設定内容がKrileの設定に反映されます。" +
                "よろしいですか？", "色設定保存", System.Windows.MessageBoxImage.Warning,
                 System.Windows.MessageBoxButton.YesNo, "Confirm");
            this.Messenger.Raise(cm);
            if (cm.Response.GetValueOrDefault())
            {
                var sfm = new SavingFileSelectionMessage("SaveFile");
                sfm.Title = "色設定ファイルの保存";
                sfm.Filter = "色設定ファイル|*.kcx";
                this.Messenger.Raise(sfm);
                if (sfm.Response != null)
                {
                    try
                    {
                        Apply();
                        XMLSerializer.SaveXML<ColoringProperty>(sfm.Response, Setting.Instance.ColoringProperty);
                    }
                    catch
                    {
                        this.Messenger.Raise(new InformationMessage("ファイルを読み込めません。",
                            "色設定ファイルのロードエラー", System.Windows.MessageBoxImage.Error, "Message"));
                    }
                }
            }
        }
        #endregion

        public void Apply()
        {
            NameBackColors
                .Concat(TextBackColors)
                .Concat(TextForeColors)
                .Concat(CommonColors)
                .ForEach(a => a.Apply());
            Setting.Instance.ColoringProperty.TweetColorMode = (TweetColoringMode)this._coloringIndex;
            Setting.Instance.ColoringProperty.SetInputCaretColorWhite = SetInputCaretColorWhite;
            Setting.Instance.ColoringProperty.SetSearchCaretColorWhite = SetSearchCaretColorWhite;
            Setting.Instance.TimelineExperienceProperty.BackgroundImage = this.BackgroundImage;
        }
    }

    public class ColorElementViewModel : ViewModel, IApplyable
    {
        public ColorElementViewModel(string description, ColorElement element)
        {
            this._element = element;
            this._description = description;
            this._cPBViewModel = new ColorPickButtonViewModel(element.GetColor());
        }

        private ColorElement _element;

        private string _description;
        public string Description { get { return this._description; } }

        ColorPickButtonViewModel _cPBViewModel;
        public ColorPickButtonViewModel CPBViewModel { get { return this._cPBViewModel; } }

        public virtual void Apply()
        {
            this._element.R = (byte)this._cPBViewModel.RValue;
            this._element.G = (byte)this._cPBViewModel.GValue;
            this._element.B = (byte)this._cPBViewModel.BValue;
            this._element.A = (byte)this._cPBViewModel.AValue;
        }
    }

    public class DisablableColorElementViewModel : ColorElementViewModel
    {
        public DisablableColorElementViewModel(string description, DisablableColorElement element)
            : base(description, element)
        {
            this._delement = element;
            this.IsActivated = element.IsActivated;
        }

        private DisablableColorElement _delement;

        private bool _isActivated = false;
        public bool IsActivated
        {
            get { return _isActivated; }
            set
            {
                this._isActivated = value;
                RaisePropertyChanged(() => IsActivated);
            }
        }

        public override void Apply()
        {
            base.Apply();
            _delement.IsActivated = IsActivated;
        }
    }

    public class PairColorElementViewModel : ViewModel, IApplyable
    {
        public PairColorElementViewModel(string description, PairColorElement pelem)
        {
            this._description = description;
            this._lightViewModel = new ColorElementViewModel(description, pelem.LightColor);
            this._darkViewModel = new ColorElementViewModel(description, pelem.DarkColor);
        }

        private string _description;
        public string Description { get { return this._description; } }

        public ColorElementViewModel _lightViewModel;
        public ColorElementViewModel LightViewModel { get { return this._lightViewModel; } }

        public ColorElementViewModel _darkViewModel;
        public ColorElementViewModel DarkViewModel { get { return this._darkViewModel; } }

        public virtual void Apply()
        {
            this._lightViewModel.Apply();
            this._darkViewModel.Apply();
        }
    }

    public class DisablablePairColorElementViewModel : ViewModel, IApplyable
    {
        public DisablablePairColorElementViewModel(string description, DisablablePairColorElement pelem)
        {
            this._description = description;
            this._lightViewModel = new DisablableColorElementViewModel(description, pelem.LightColor);
            this._darkViewModel = new DisablableColorElementViewModel(description, pelem.DarkColor);
        }

        private string _description;
        public string Description { get { return this._description; } }

        public DisablableColorElementViewModel _lightViewModel;
        public DisablableColorElementViewModel LightViewModel { get { return this._lightViewModel; } }

        public DisablableColorElementViewModel _darkViewModel;
        public DisablableColorElementViewModel DarkViewModel { get { return this._darkViewModel; } }

        public void Apply()
        {
            _lightViewModel.Apply();
            _darkViewModel.Apply();
        }
    }
}
