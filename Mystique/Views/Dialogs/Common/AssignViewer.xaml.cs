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
using Inscribe.ViewModels.Dialogs.Common;
using Inscribe.Configuration.KeyAssignment;

namespace Mystique.Views.Dialogs.Common
{
    /// <summary>
    /// AssignViewer.xaml の相互作用ロジック
    /// </summary>
    public partial class AssignViewer : Window
    {
        public AssignViewer()
        {
            InitializeComponent();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var dc = this.DataContext as AssignViewerViewModel;
            if (dc != null)
                dc.InputKey = KeyAssign.KeyToString(Keyboard.Modifiers, e.Key);
        }
    }
}