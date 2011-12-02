using System;
using Inscribe.ViewModels.Behaviors.Messaging;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;

namespace Inscribe.ViewModels.PartBlocks.MainBlock
{
    public abstract class TimelineCoreViewModelBase : ViewModel
    {
        public abstract TimelineListCoreViewModel CoreViewModel { get; }

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
