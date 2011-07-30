using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.ViewModels.Behaviors.Messaging;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;

namespace Inscribe.ViewModels.PartBlocks.MainBlock
{
    public abstract class TimelineCoreViewModelBase : ViewModel
    {
        public abstract bool IsActive { get; }

        public abstract void InvalidateIsActive();

        /// <summary>
        /// このタイムラインがフォーカスを得た
        /// </summary>
        public event EventHandler GotFocus;
        protected void OnGetFocus()
        {

            var fchandler = GotFocus;
            if (fchandler != null)
                fchandler(this, EventArgs.Empty);
        }

        public abstract void SetSelect(ListSelectionKind kind);

        public abstract TabDependentTweetViewModel SelectedTweetViewModel { get; set; }

        public abstract void InvalidateCache();
    }
}
