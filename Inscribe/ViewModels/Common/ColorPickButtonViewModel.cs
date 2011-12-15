using System.Windows.Media;
using Livet;

namespace Inscribe.ViewModels.Common
{
    public class ColorPickButtonViewModel : ViewModel
    {
        public ColorPickButtonViewModel(Color curColor)
        {
            this._currentColor = curColor;
        }

        private Color _currentColor;

        public Color CurrentColor
        {
            get { return this._currentColor; }
            set
            {
                this._currentColor = value;
                RaisePropertyChanged(() => CurrentColor);
                RaisePropertyChanged(() => CurrentColorBrush);
                RaisePropertyChanged(() => RString);
                RaisePropertyChanged(() => RValue);
                RaisePropertyChanged(() => GString);
                RaisePropertyChanged(() => GValue);
                RaisePropertyChanged(() => BString);
                RaisePropertyChanged(() => BValue);
            }
        }

        public Brush CurrentColorBrush
        {
            get
            {
                return new SolidColorBrush(this.CurrentColor);
            }
        }

        public string RString
        {
            get { return RValue.ToString(); }
            set
            {
                byte pv;
                if (byte.TryParse(value, out pv))
                    this.RValue = pv;
                else
                    this.RValue = 0;
            }
        }
        public int RValue
        {
            get { return this._currentColor.R; }
            set
            {
                this._currentColor.R = (byte)value;
                RaisePropertyChanged(() => RValue);
                RaisePropertyChanged(() => RString);
                RaisePropertyChanged(() => CurrentColor);
                RaisePropertyChanged(() => CurrentColorBrush);
            }
        }

        public string GString
        {
            get { return GValue.ToString(); }
            set
            {
                byte pv;
                if (byte.TryParse(value, out pv))
                    this.GValue = pv;
                else
                    this.GValue = 0;
            }
        }
        public int GValue
        {
            get { return this._currentColor.G; }
            set
            {
                this._currentColor.G = (byte)value;
                RaisePropertyChanged(() => GValue);
                RaisePropertyChanged(() => GString);
                RaisePropertyChanged(() => CurrentColor);
                RaisePropertyChanged(() => CurrentColorBrush);
            }
        }

        public string BString
        {
            get { return BValue.ToString(); }
            set
            {
                byte pv;
                if (byte.TryParse(value, out pv))
                    this.BValue = pv;
                else
                    this.BValue = 0;
            }
        }
        public int BValue
        {
            get { return this._currentColor.B; }
            set
            {
                this._currentColor.B = (byte)value;
                RaisePropertyChanged(() => BValue);
                RaisePropertyChanged(() => BString);
                RaisePropertyChanged(() => CurrentColor);
                RaisePropertyChanged(() => CurrentColorBrush);
            }
        }
    }
}