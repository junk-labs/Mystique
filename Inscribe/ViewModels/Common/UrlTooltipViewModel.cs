using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using System.Threading.Tasks;
using Inscribe.Configuration;
using Inscribe.Plugin;

namespace Inscribe.ViewModels.Common
{
    public class UrlTooltipViewModel : ViewModel
    {
        private string url;

        public UrlTooltipViewModel(string url)
        {
            this.url = url;
        }

        private bool isBeginResolved = false;
        public void BeginResolve()
        {
            if (isBeginResolved) return;
            isBeginResolved = true;
            Task.Factory.StartNew(() =>
            {
                if (Setting.Instance.TweetExperienceProperty.UrlResolving == Configuration.Settings.TweetExperienceProperty.UrlResolveStrategy.OnPointed)
                    url = ShortenManager.Extract(url);
                ResolvedUrl = url;
                ImageLinkUrl = UploaderManager.TryResolve(url);
            });
        }

        public bool IsResolved
        {
            get
            {
                return _resolvedUrl != null;
            }
        }

        private string _resolvedUrl = null;
        public string ResolvedUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_resolvedUrl))
                {
                    return "(展開に失敗しました。)";
                }
                else
                {
                    return _resolvedUrl;
                }
            }
            private set
            {
                DispatcherHelper.BeginInvoke(()=>
                {
                    _resolvedUrl = value;
                    RaisePropertyChanged(() => ResolvedUrl);
                    RaisePropertyChanged(() => IsResolved);
                });
            }
        }

        private string _imageLinkUrl = null;
        public string ImageLinkUrl
        {
            get { return _imageLinkUrl; }
            private set
            {
                DispatcherHelper.BeginInvoke(()=>
                {
                    _imageLinkUrl = value;
                    RaisePropertyChanged(() => ImageLinkUrl);
                    RaisePropertyChanged(() => IsImageLink);
                });
            }
        }

        public bool IsImageLink
        {
            get
            {
                return ImageLinkUrl != null;
            }
        }
    }
}
