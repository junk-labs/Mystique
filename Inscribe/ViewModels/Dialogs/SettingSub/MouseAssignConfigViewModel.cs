using System;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class MouseAssignConfigViewModel : ViewModel, IApplyable
    {
        public MouseAssignConfigViewModel()
        {
            isMouseAssignEnabled = Setting.Instance.MouseAssignProperty.IsMouseAssignEnabled;
            replyActionSetViewModel = new ActionSetViewModel<ReplyMouseActionCandidates>(Setting.Instance.MouseAssignProperty.ReplyActionSet);
            favActionSetViewModel = new ActionSetViewModel<FavMouseActionCandidates>(Setting.Instance.MouseAssignProperty.FavActionSet);
            retweetActionSetViewModel = new ActionSetViewModel<RetweetMouseActionCandidates>(Setting.Instance.MouseAssignProperty.RetweetActionSet);
            unofficialRetweetActionSetViewModel = new ActionSetViewModel<UnofficialRetweetQuoteMouseActionCandidates>(Setting.Instance.MouseAssignProperty.UnofficialRetweetActionSet);
            quoteTweetActionSetViewModel = new ActionSetViewModel<UnofficialRetweetQuoteMouseActionCandidates>(Setting.Instance.MouseAssignProperty.QuoteTweetActionSet);
        }

        public bool IsAloofUserMode
        {
            get { return Setting.Instance.ExperienceProperty.IsAloofUserMode; }
        }

        private bool isMouseAssignEnabled;
        public bool IsMouseAssignEnabled
        {
            get { return isMouseAssignEnabled; }
            set
            {
                isMouseAssignEnabled = value;
                RaisePropertyChanged(() => IsMouseAssignEnabled);
            }
        }

        private ActionSetViewModel<ReplyMouseActionCandidates> replyActionSetViewModel;
        public ActionSetViewModel<ReplyMouseActionCandidates> ReplyActionSetViewModel
        {
            get { return replyActionSetViewModel; }
            set { replyActionSetViewModel = value; }
        }

        private ActionSetViewModel<FavMouseActionCandidates> favActionSetViewModel;
        public ActionSetViewModel<FavMouseActionCandidates> FavActionSetViewModel
        {
            get { return favActionSetViewModel; }
            set { favActionSetViewModel = value; }
        }

        private ActionSetViewModel<RetweetMouseActionCandidates> retweetActionSetViewModel;
        public ActionSetViewModel<RetweetMouseActionCandidates> RetweetActionSetViewModel
        {
            get { return retweetActionSetViewModel; }
            set { retweetActionSetViewModel = value; }
        }

        private ActionSetViewModel<UnofficialRetweetQuoteMouseActionCandidates> unofficialRetweetActionSetViewModel;
        public ActionSetViewModel<UnofficialRetweetQuoteMouseActionCandidates> UnofficialRetweetActionSetViewModel
        {
            get { return unofficialRetweetActionSetViewModel; }
            set { unofficialRetweetActionSetViewModel = value; }
        }

        private ActionSetViewModel<UnofficialRetweetQuoteMouseActionCandidates> quoteTweetActionSetViewModel;
        public ActionSetViewModel<UnofficialRetweetQuoteMouseActionCandidates> QuoteTweetActionSetViewModel
        {
            get { return quoteTweetActionSetViewModel; }
            set { quoteTweetActionSetViewModel = value; }
        }

        public void Apply()
        {
            Setting.Instance.MouseAssignProperty.IsMouseAssignEnabled = isMouseAssignEnabled;
            Setting.Instance.MouseAssignProperty.ReplyActionSet = replyActionSetViewModel.GetActionSet();
            Setting.Instance.MouseAssignProperty.FavActionSet = favActionSetViewModel.GetActionSet();
            Setting.Instance.MouseAssignProperty.RetweetActionSet = retweetActionSetViewModel.GetActionSet();
            Setting.Instance.MouseAssignProperty.UnofficialRetweetActionSet = unofficialRetweetActionSetViewModel.GetActionSet();
            Setting.Instance.MouseAssignProperty.QuoteTweetActionSet = quoteTweetActionSetViewModel.GetActionSet();
        }
    }

    public class ActionSetViewModel<T> : ViewModel
        where T : struct
    {
        public ActionSetViewModel(ActionSet<T> actionSet)
        {
            noneKeyActionViewModel = new ActionViewModel<T>(actionSet.NoneKeyAction);
            controlKeyActionViewModel = new ActionViewModel<T>(actionSet.ControlKeyAction);
            altKeyActionViewModel = new ActionViewModel<T>(actionSet.AltKeyAction);
            shiftKeyActionViewModel = new ActionViewModel<T>(actionSet.ShiftKeyAction);
        }

        ActionViewModel<T> noneKeyActionViewModel;
        public ActionViewModel<T> NoneKeyActionViewModel
        {
            get { return noneKeyActionViewModel; }
            set { noneKeyActionViewModel = value; }
        }

        ActionViewModel<T> controlKeyActionViewModel;
        public ActionViewModel<T> ControlKeyActionViewModel
        {
            get { return controlKeyActionViewModel; }
            set { controlKeyActionViewModel = value; }
        }

        ActionViewModel<T> altKeyActionViewModel;
        public ActionViewModel<T> AltKeyActionViewModel
        {
            get { return altKeyActionViewModel; }
            set { altKeyActionViewModel = value; }
        }

        ActionViewModel<T> shiftKeyActionViewModel;
        public ActionViewModel<T> ShiftKeyActionViewModel
        {
            get { return shiftKeyActionViewModel; }
            set { shiftKeyActionViewModel = value; }
        }

        public ActionSet<T> GetActionSet()
        {
            return new ActionSet<T>()
            {
                NoneKeyAction = noneKeyActionViewModel.GetActionDescription(),
                ControlKeyAction = controlKeyActionViewModel.GetActionDescription(),
                AltKeyAction = altKeyActionViewModel.GetActionDescription(),
                ShiftKeyAction = shiftKeyActionViewModel.GetActionDescription()
            };
        }
    }

    public class ActionViewModel<T> : ViewModel
        where T : struct
    {
        public ActionViewModel(ActionDescription<T> description)
        {
            if (description != null)
            {
                _actionDescriptionIndex = Convert.ToInt32(description.Action);
                _argument = description.ActionArgs;
            }
            else
            {
                _actionDescriptionIndex = 0;
                _argument = String.Empty;
            }
        }

        private int _actionDescriptionIndex;
        public int ActionDescriptionIndex
        {
            get { return _actionDescriptionIndex; }
            set
            {
                _actionDescriptionIndex = value;
                RaisePropertyChanged(() => ActionDescriptionIndex);
            }
        }

        private string _argument;
        public string Argument
        {
            get { return _argument; }
            set
            {
                _argument = value;
                RaisePropertyChanged(() => Argument);
            }
        }

        public ActionDescription<T> GetActionDescription()
        {
            return new ActionDescription<T>((T)Enum.ToObject(typeof(T), ActionDescriptionIndex), Argument);
        }
    }
}
