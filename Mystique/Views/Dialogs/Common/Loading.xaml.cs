using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mystique.Views.Dialogs.Common
{
    /// <summary>
    /// Loading.xaml の相互作用ロジック
    /// </summary>
    public partial class Loading : Window
    {
        public Loading()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var dc = this.DataContext as Mystique.ViewModels.Dialogs.Common.LoadingViewModel;
            dc.LoadedCommand.Execute();
        }
    }
}