using System;
using System.Linq;
using Acuerdo.External.Shortener;
using Acuerdo.External.Uploader;
using Inscribe.Configuration;
using Inscribe.Plugin;
using Livet;

namespace Inscribe.ViewModels.Dialogs.SettingSub
{
    public class ExtServiceConfigViewModel : ViewModel, IApplyable
    {
        public ExtServiceConfigViewModel()
        {
            this.UrlShortenerCandidates = ShortenManager.Shorteners.ToArray();
            this.ImageUploaderCandidates = UploaderManager.Uploaders.ToArray();

            var ss = ShortenManager.GetSuggestedShortener();
            this.UrlCompressCandidateIndex = -1;
            if (ss != null)
            {
                var idx = this.UrlShortenerCandidates
                    .TakeWhile(u => u.Name != Setting.Instance.ExternalServiceProperty.ShortenerService)
                    .Count();
                if (idx < this.UrlShortenerCandidates.Length)
                {
                    this.UrlCompressCandidateIndex = idx;
                }
            }
            if (this.UrlCompressCandidateIndex == -1)
                this.UrlCompressCandidateIndex = 0;

            var su = UploaderManager.GetSuggestedUploader();
            this.ImageUploadCandidateIndex = -1;
            if (su != null)
            {
                var idx = this.ImageUploaderCandidates
                    .TakeWhile(u => u.ServiceName != Setting.Instance.ExternalServiceProperty.UploaderService)
                    .Count();
                if (idx < this.ImageUploaderCandidates.Length)
                {
                    this.ImageUploadCandidateIndex = idx;
                }
            }
            if (this.ImageUploadCandidateIndex == -1)
                this.ImageUploadCandidateIndex = 0;
        }

        public IURLShortener[] UrlShortenerCandidates { get; private set; }

        private int _urlCompressCandidateIndex;
        public int UrlCompressCandidateIndex
        {
            get { return this._urlCompressCandidateIndex; }
            set
            {
                this._urlCompressCandidateIndex = value;
                RaisePropertyChanged(() => UrlCompressCandidateIndex);
            }
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

        public void Apply()
        {
            if (this.UrlCompressCandidateIndex < this.UrlShortenerCandidates.Count())
                Setting.Instance.ExternalServiceProperty.ShortenerService =
                    this.UrlShortenerCandidates[this.UrlCompressCandidateIndex].Name;
            else
                Setting.Instance.ExternalServiceProperty.ShortenerService = String.Empty;

            if (this.ImageUploadCandidateIndex < this.ImageUploaderCandidates.Count())
                Setting.Instance.ExternalServiceProperty.UploaderService =
                    this.ImageUploaderCandidates[this.ImageUploadCandidateIndex].ServiceName;
            else
                Setting.Instance.ExternalServiceProperty.UploaderService = String.Empty;
        }
    }
}
