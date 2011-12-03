using System.Threading;
using System.Threading.Tasks;
using Inscribe.Configuration;
using Inscribe.Storage;
using Livet;
using Inscribe.Util;

namespace Inscribe.ViewModels.PartBlocks.BlockCommon
{
    public class NotifierViewModel : ViewModel
    {
        public MainWindowViewModel Parent { get; private set; }

        public NotifierViewModel(MainWindowViewModel parent)
        {
            this.Parent = parent;
            ViewModelHelper.BindNotification(EventStorage.EventRegisteredEvent, this, OnEventRegistered);
        }

        private void OnEventRegistered(object o, EventDescriptionEventArgs e)
        {
            if (e.EventDescription == null || !Setting.Instance.NotificationProperty.IsEnabledNotificationBar) return;
            switch (e.EventDescription.Kind)
            {
                case EventKind.DirectMessage:
                    if (!Setting.Instance.NotificationProperty.NotifyDmEvent) return;
                    break;
                case EventKind.Favorite:
                case EventKind.Unfavorite:
                    if (!Setting.Instance.NotificationProperty.NotifyFavoriteEvent) return;
                    break;
                case EventKind.Mention:
                    if (!Setting.Instance.NotificationProperty.NotifyMentionEvent) return;
                    break;
                case EventKind.Retweet:
                    if (!Setting.Instance.NotificationProperty.NotifyRetweetEvent) return;
                    break;
                case EventKind.Follow:
                    if (!Setting.Instance.NotificationProperty.NotifyFollowEvent) return;
                    break;
            }
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
