using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.ViewModels.Common;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class ColoringConfigViewModel : ViewModel, IApplyable
    {
        public IEnumerable<IApplyable> Applyables { get; private set; }

        public ColoringConfigViewModel()
        {
            Applyables = new IApplyable[]{
                Wrap(Setting.Instance.ColoringProperty.BaseColor, "基本色"),
                Wrap(Setting.Instance.ColoringProperty.Selected, "選択ツイートと同じユーザーのツイート"),
                Wrap(Setting.Instance.ColoringProperty.Retweeted, "リツイートされたツイート"),
                Wrap(Setting.Instance.ColoringProperty.DirectMessage, "アクティブアカウントのDM"),
                Wrap(Setting.Instance.ColoringProperty.DirectMessageToSub, "全アカウントのDM"),
                Wrap(Setting.Instance.ColoringProperty.BaseHighlightColor, "基本ハイライト色"),
                Wrap(Setting.Instance.ColoringProperty.Follower, "アクティブアカウントの片思われ"),
                Wrap(Setting.Instance.ColoringProperty.FollowerAny, "全アカウントの片思われ"),
                Wrap(Setting.Instance.ColoringProperty.Following, "アクティブアカウントの片思い"),
                Wrap(Setting.Instance.ColoringProperty.FollowingAny, "全アカウントの片思い"),
                Wrap(Setting.Instance.ColoringProperty.Friend, "アクティブアカウントの両想い"),
                Wrap(Setting.Instance.ColoringProperty.FriendAny, "全アカウントの両想い"),
                Wrap(Setting.Instance.ColoringProperty.InReplyToMeCurrent, "アクティブアカウントへの返信"),
                Wrap(Setting.Instance.ColoringProperty.InReplyToMeSub, "全アカウントへの返信"),
                Wrap(Setting.Instance.ColoringProperty.MyCurrentTweet, "アクティブアカウントのツイート"),
                Wrap(Setting.Instance.ColoringProperty.MySubTweet, "全アカウントのツイート"),
            };
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

        public void Apply()
        {
            foreach (var a in Applyables)
                a.Apply();
        }
    }

    public class ColorElementViewModel : ViewModel, IApplyable
    {
        public ColorElementViewModel(string description, ColorElement element)
        {
            this._element = element;
            this._description = description;
            this._cPBViewModel = new ColorPickButtonViewModel(element.GetColor(), element.GetDefaultColor());
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
