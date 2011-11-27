using System;
using System.Linq;
using Acuerdo.External.Uploader;
using Inscribe.Configuration;
using Inscribe.Plugin;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class ExternalConfigViewModel : ViewModel, IApplyable
    {
        public ExternalConfigViewModel()
        {
            this.ImageUploaderCandidates = UploaderManager.Uploaders.ToArray();

            var su = UploaderManager.GetSuggestedUploader();
            this.ImageUploadCandidateIndex = -1;
            if (su != null)
            {
                var idx = this.ImageUploaderCandidates
                    .TakeWhile(u => u.ServiceName != Setting.Instance.ExternalProperty.UploaderService)
                    .Count();
                if (idx < this.ImageUploaderCandidates.Length)
                {
                    this.ImageUploadCandidateIndex = idx;
                }
            }
            if (this.ImageUploadCandidateIndex == -1)
                this.ImageUploadCandidateIndex = 0;
        }

        public IUploader[] ImageUploaderCandidates { get; private set; }

        private int _imageUploadCandidateIndex;
        public int ImageUploadCandidateIndex
        {
            get { return this._imageUploadCandidateIndex; }
            set
            {
                this._imageUploadCandidateIndex = value;
                RaisePropertyChanged(() => ImageUploadCandidateIndex);
            }
        }

        private string _webBrowserPath;
        public string WebBrowserPath
        {
            get { return _webBrowserPath; }
            set
            {
                _webBrowserPath = value;
                RaisePropertyChanged(() => WebBrowserPath);
            }
        }

        private string _webBrowserParam;
        public string WebBrowserParam
        {
            get { return _webBrowserParam; }
            set
            {
                _webBrowserParam = value;
                RaisePropertyChanged(() => WebBrowserParam);
            }
        }

        public void Apply()
        {
            if (this.ImageUploadCandidateIndex < this.ImageUploaderCandidates.Count())
                Setting.Instance.ExternalProperty.UploaderService =
                    this.ImageUploaderCandidates[this.ImageUploadCandidateIndex].ServiceName;
            else
                Setting.Instance.ExternalProperty.UploaderService = String.Empty;
            Setting.Instance.ExternalProperty.WebBrowserPath = this.WebBrowserPath;
            Setting.Instance.ExternalProperty.WebBrowserParam = this.WebBrowserParam;
        }
    }
}
