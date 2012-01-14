using System;
using Inscribe.Authentication;
using Livet;

namespace Inscribe.ViewModels.Common
{
    public class ProfileImageProvider : ViewModel
    {
        private AccountInfo _info;
        public ProfileImageProvider(AccountInfo relatedInfo)
        {
            this._info = relatedInfo;
        }

        public Uri ProfileImage
        {
            get
            {
                var ud = this._info.UserViewModel;
                if (ud != null)
                    return ud.TwitterUser.ProfileImage;
                else
                    return null;
            }
        }
    }
}
