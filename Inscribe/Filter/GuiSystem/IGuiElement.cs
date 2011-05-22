using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.GuiSystem
{
    public interface IGuiElement
    {
        /// <summary>
        /// GUI編集用の一行説明を取得します。
        /// </summary>
        string Description { get; }

        /// <summary>
        /// このフィルタの設定状態を示す一行説明を取得します。
        /// </summary>
        string FilterStateString { get; }
    }
}
