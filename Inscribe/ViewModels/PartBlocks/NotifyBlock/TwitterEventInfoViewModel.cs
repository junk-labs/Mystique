using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Storage;
using Livet.Commands;
using Inscribe.ViewModels.PartBlocks.BlockCommon;

namespace Inscribe.ViewModels.PartBlocks.NotifyBlock
{
    public class TwitterEventInfoViewModel : ViewModel
    {
        public TwitterEventInfoViewModel()
        {
            ViewModelHelper.BindNotification(EventStorage.EventRegisteredEvent, this, (o, e) => RaisePropertyChanged(() => Events));
        }

        public IEnumerable<NotifierViewModel> Events
        {
            get { return EventStorage.Events.Select(e => new NotifierViewModel(e, false)); }
        }
    }
}
