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
using Inscribe.Configuration.KeyAssignment;

namespace Mystique.Views.PartBlocks.InputBlock
{
    /// <summary>
    /// InputBlock.xaml の相互作用ロジック
    /// </summary>
    public partial class InputBlock : UserControl
    {
        public InputBlock()
        {
            InitializeComponent();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeyAssign.HandlePreviewEvent(e, AssignRegion.Input);
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            KeyAssign.HandleEvent(e, AssignRegion.Input);
        }
    }
}
