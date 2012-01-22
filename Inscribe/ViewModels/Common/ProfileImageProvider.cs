using System;
using Inscribe.Authentication;
using Livet;
using Inscribe.Storage;
using System.Threading.Tasks;

namespace Inscribe.ViewModels.Common
{
    public class ProfileImageProvider : ViewModel
    {
        private AccountInfo _info;
        private bool? _fetching = null;
        public ProfileImageProvider(AccountInfo relatedInfo)
        {
            this._info = relatedInfo;
        }

        public Uri ProfileImage
        {
            get
            {
                if (_fetching.HasValue && _fetching.Value == true) return null;
                if (UserStorage.Lookup(_info.NumericId) != null || !_fetching.GetValueOrDefault())
                {
                    var ud = this._info.UserViewModel;
                    if (ud != null)
                        return ud.TwitterUser.ProfileImage;
                    else
                        return null;
                }
                else
                {
                    _fetching = true;
                    Task.Factory.StartNew(() =>
                    {
                        var info = this._info.UserViewModel;
                        RaisePropertyChanged(() => ProfileImage);
                        _fetching = false;
                    });
                    return null;
                }
            }
        }
    }
}
