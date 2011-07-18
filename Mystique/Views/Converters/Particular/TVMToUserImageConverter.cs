using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.ViewModels;
using System.Windows.Data;
using Dulcet.Twitter;
using Inscribe.Common;
using Inscribe.ViewModels.Timeline;

namespace Mystique.Views.Converters.Particular
{
    public enum UserImageViewKind
    {
        Default,
        Suggested,
        Retweeted,
        DirectMessageRecipient,
    }

    public class TVMToUserImageConverter : OneWayConverter<TweetViewModel, Uri>
    {
        public override Uri ToTarget(TweetViewModel input, object parameter)
        {
            if (input == null) return null;
            UserImageViewKind kind;
            if (!Enum.TryParse(parameter as string, out kind))
                kind = UserImageViewKind.Default;
            switch (kind)
            {
                case UserImageViewKind.Default:
                case UserImageViewKind.Retweeted:
                    return input.Status.User.ProfileImage;
                case UserImageViewKind.DirectMessageRecipient:
                    var dm = input.Status as TwitterDirectMessage;
                    if (dm == null)
                        return null;
                    else
                        return dm.Recipient.ProfileImage;
                case UserImageViewKind.Suggested:
                    return TwitterHelper.GetSuggestedUser(input).ProfileImage;
                default:
                    return null;
            }
        }
    }
}
