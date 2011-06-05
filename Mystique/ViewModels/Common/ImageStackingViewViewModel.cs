using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;

namespace Mystique.ViewModels.Common
{
    public class ImageStackingViewViewModel : ViewModel
    {
        public IEnumerable<ImageStackItem> Images
        {
            get { return Layout(this._images ?? new Uri[0]); }
        }

        private IEnumerable<ImageStackItem> Layout(IEnumerable<Uri> uris)
        {
            int count = 0;
            foreach (var item in uris)
            {
                yield return new ImageStackItem(count * 10.0, item);
            }
        }

        private IEnumerable<Uri> _images = null;
        public IEnumerable<Uri> ImageUrls
        {
            get { return this._images; }
            set
            {
                this._images = value;
                RaisePropertyChanged(() => ImageUrls);
                RaisePropertyChanged(() => Images);
            }
        }
    }

    public class ImageStackItem : ViewModel
    {
        public ImageStackItem(double slideRate, Uri imageSource)
        {
            this.SlideMoveRate = slideRate;
            this.ImageSource = imageSource;
        }

        public double SlideMoveRate { get; set; }

        public Uri ImageSource { get; set; }

    }
}
