using System;
using Inscribe.Authentication;
using Livet;
using Inscribe.Storage;
using System.Threading.Tasks;
using Inscribe.Common;

namespace Inscribe.ViewModels.Common
{
    public class ProfileImageProvider : ViewModel
    {
        private static StackTaskDispatcher imageloader;
        static ProfileImageProvider()
        {
            imageloader = new StackTaskDispatcher(1);
            ThreadHelper.Halt += () => imageloader.Dispose();
        }

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
                if (_fetching.GetValueOrDefault()) return null;
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
                    imageloader.Push(()=>
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
