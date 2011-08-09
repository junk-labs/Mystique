using System.Windows;
using System.Windows.Input;
using Inscribe.ViewModels.Dialogs.Common;
using Inscribe.Subsystems;

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
                dc.InputKey = KeyAssignCore.KeyToString(Keyboard.Modifiers, e.Key);
        }
    }
}