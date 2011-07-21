using System.Threading;
using System.Threading.Tasks;
using Inscribe.Configuration;
using Inscribe.Storage;
using Livet;
using Inscribe.Data;

namespace Inscribe.ViewModels.PartBlocks.BlockCommon
{
    public class NotifierViewModel : ViewModel
    {
        public NotifierViewModel()
        {
            ViewModelHelper.BindNotification(EventStorage.EventRegisteredEvent, this, OnEventRegistered);
        }

        private void OnEventRegistered(object o, EventDescriptionEventArgs e)
        {
            var nivm = new NotificationItemViewModel(e.EventDescription);
            nivm.RequireClose += (no, ne) => _notifications.Remove(nivm);
            _notifications.Add(nivm);
            Task.Factory.StartNew(() => Thread.Sleep(Setting.Instance.ExperienceProperty.TwitterActionNotifyShowLength))
                .ContinueWith(_ => _notifications.Remove(nivm));

        }

        ConcurrentObservable<NotificationItemViewModel> _notifications = new ConcurrentObservable<NotificationItemViewModel>();
        public ConcurrentObservable<NotificationItemViewModel> Notifications
        {
            get { return _notifications; }
        }
    }
}
