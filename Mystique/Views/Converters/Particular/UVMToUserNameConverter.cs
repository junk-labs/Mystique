using System;
using System.Windows.Data;
using Inscribe.Configuration;
using Inscribe.Configuration.Settings;
using Inscribe.ViewModels.PartBlocks.MainBlock;

namespace Mystique.Views.Converters.Particular
{
    public class UVMToUserNameConverter : OneWayConverter<UserViewModel, string>
    {
        public override string ToTarget(UserViewModel input, object parameter)
        {
            if (input == null) return String.Empty;
            UserNameViewKind kind;
            if (!Enum.TryParse(parameter as string, out kind))
                kind = UserNameViewKind.ScreenName;
            switch (kind)
            {
                case UserNameViewKind.Name:
                    return UserName(input);
                case UserNameViewKind.ScreenName:
                    return ScreenName(input);
                case UserNameViewKind.RetweetedScreenName:
                    if (input == null) return String.Empty;
                    return input.TwitterUser.ScreenName;
                case UserNameViewKind.ViewName:
                    switch (Setting.Instance.TweetExperienceProperty.UserNameViewMode)
                    {
                        case NameView.ID:
                            return ScreenName(input);
                        case NameView.Name:
                            return UserName(input);
                        case NameView.Both:
                        default:
                            return ScreenName(input) + " (" + UserName(input) + ")";
                    }
                case UserNameViewKind.NotifyViewName:
                    switch (Setting.Instance.TweetExperienceProperty.NotificationNameViewMode)
                    {
                        case NameView.ID:
                            return ScreenName(input);
                        case NameView.Name:
                            return UserName(input);
                        case NameView.Both:
                        default:
                            return ScreenName(input) + " (" + UserName(input) + ")";
                    }
                default:
                    return String.Empty;
            }
        }

        private static string UserName(UserViewModel user)
        {
            return user.TwitterUser.UserName;
        }

        private static string ScreenName(UserViewModel user)
        {
            return user.TwitterUser.ScreenName;
        }
    }
}
