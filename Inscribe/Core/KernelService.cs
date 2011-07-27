using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.ViewModels;

namespace Inscribe.Core
{
    public static class KernelService
    {
        private static MainWindowViewModel _mainWindowViewModel = null;
        public static MainWindowViewModel MainWindowViewModel
        {
            get { return _mainWindowViewModel; }
            set
            {
                if (_mainWindowViewModel == null)
                    _mainWindowViewModel = value;
                else
                    throw new FieldAccessException("Field is already initialized.");
            }
        }
    }
}
