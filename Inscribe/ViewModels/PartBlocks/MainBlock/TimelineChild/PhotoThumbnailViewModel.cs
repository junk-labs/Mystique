using System;
using Inscribe.Common;
using Livet;

namespace Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild
{
    public class PhotoThumbnailViewModel : ViewModel
    {
        public PhotoThumbnailViewModel(Uri uri)
        {
            this.PhotoUri = uri;
        }

        public Uri PhotoUri { get; private set; }

        #region OpenUriCommand
        private Livet.Commands.ViewModelCommand _OpenUriCommand;

        public Livet.Commands.ViewModelCommand OpenUriCommand
        {
            get
            {
                if (_OpenUriCommand == null)
                {
                    _OpenUriCommand = new Livet.Commands.ViewModelCommand(OpenUri);
                }
                return _OpenUriCommand;
            }
        }

        public void OpenUri()
        {
            if (PhotoUri == null)
            {
                return;
            }

            string uri;
            // このあたりの実装をエレガントにしたい。
            if ((PhotoUri.Host == "pbs.twimg.com") && (!PhotoUri.AbsolutePath.EndsWith(":orig")))
            {
                uri = PhotoUri.OriginalString + ":orig";
            }
            else
            {
                uri = PhotoUri.OriginalString;
            }
            Browser.Start(uri);
        }
        #endregion

    }
}
