using System;
using System.Windows.Data;
using Dulcet.Twitter;
using Inscribe.Common;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using System.Windows;

namespace Mystique.Views.Converters.Particular
{
    public enum UserImageViewKind
    {
        Default,
        Suggested,
        Retweeted,
        DirectMessageRecipient,
    }

    public class TVMToUserImageConverter : OneWayConverter<TweetViewModel, object>
    {
        public override object ToTarget(TweetViewModel input, object parameter)
        {
            if (input == null) return DependencyProperty.UnsetValue;
            UserImageViewKind kind;
            if (!Enum.TryParse(parameter as string, out kind))
                kind = UserImageViewKind.Default;
            switch (kind)
            {
                case UserImageViewKind.Default:
                case UserImageViewKind.Retweeted:
                    return input.Status.User.ProfileImage ?? DependencyProperty.UnsetValue;
                case UserImageViewKind.DirectMessageRecipient:
                    var dm = input.Status as TwitterDirectMessage;
                    if (dm == null)
                        return DependencyProperty.UnsetValue;
                    else
                        return dm.Recipient.ProfileImage ?? DependencyProperty.UnsetValue;
                case UserImageViewKind.Suggested:
                    return TwitterHelper.GetSuggestedUser(input).ProfileImage ?? DependencyProperty.UnsetValue;
                default:
                    return DependencyProperty.UnsetValue;
            }
        }
    }
}
