using System.Windows.Controls;
using System.Windows.Input;
using Inscribe.Subsystems;
using Inscribe.Subsystems.KeyAssign;

namespace Mystique.Views.PartBlocks.MainBlock
{
    /// <summary>
    /// TimelineList.xaml の相互作用ロジック
    /// </summary>
    public partial class TimelineList : UserControl
    {
        public TimelineList()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeyAssignCore.HandlePreviewEvent(e, AssignRegion.SearchBar);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            KeyAssignCore.HandleEvent(e, AssignRegion.SearchBar);
        }
    }
}
