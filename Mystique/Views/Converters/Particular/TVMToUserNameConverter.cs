using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.ViewModels;
using System.Windows.Data;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Dulcet.Twitter;
using Inscribe.Common;

namespace Mystique.Views.Converters.Particular
{
    public class TVMToUserNameConverter : OneWayConverter<TweetViewModel, string>
    {
        public enum ViewKind
        {
            ScreenName,
            Name,
            ViewName,
            RetweetedScreenName,
            DirectMessageTarget
        }

        public override string ToTarget(TweetViewModel input, object parameter)
        {
            if (input == null) return String.Empty;
            var status = input;
            ViewKind kind;
            if (!Enum.TryParse<ViewKind>(parameter as string, out kind))
                kind = ViewKind.ScreenName;
            switch (kind)
            {
                case ViewKind.Name:
                    return UserName(status);
                case ViewKind.ScreenName:
                    return ScreenName(status);
                case ViewKind.RetweetedScreenName:
                    if (status == null) return String.Empty;
                    return status.Status.User.ScreenName;
                case ViewKind.ViewName:
                    switch (Setting.Instance.TweetExperienceProperty.UserNameMode)
                    {
                        case TweetExperienceProperty.NameViewMode.ID:
                            return ScreenName(status);
                        case TweetExperienceProperty.NameViewMode.Name:
                            return UserName(status);
                        case TweetExperienceProperty.NameViewMode.Both:
                            return ScreenName(status) + " (" + UserName(status) + ")";
                        default:
                            return String.Empty;
                    }
                case ViewKind.DirectMessageTarget:
                    if (status == null || !(status.Status is TwitterDirectMessage)) return String.Empty;
                    return ((TwitterDirectMessage)status.Status).Recipient.ScreenName ?? String.Empty;

                default:
                    return String.Empty;
            }
        }

        private string UserName(TweetViewModel status)
        {
            if (status == null) return String.Empty;
            return TwitterHelper.GetSuggestedUser(status).UserName ?? String.Empty;
        }

        private string ScreenName(TweetViewModel status)
        {
            if (status == null) return String.Empty;
            return TwitterHelper.GetSuggestedUser(status).ScreenName ?? String.Empty;
        }
    }
}
