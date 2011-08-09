using System.Windows.Controls;
using System.Windows.Input;
using Inscribe.Subsystems;
using Inscribe.Subsystems.KeyAssign;

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
            KeyAssignCore.HandlePreviewEvent(e, AssignRegion.Input);
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            KeyAssignCore.HandleEvent(e, AssignRegion.Input);
        }
    }
}
