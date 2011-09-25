using System;
using System.Linq;
using Livet;

namespace Inscribe.Filter
{
    /// <summary>
    /// ウィークイベントをリッスンします。
    /// </summary>
    public class WeakEventBinder<T> : IWeakEventListenerHolder
        where T : EventArgs
    {
        public WeakEventBinder(params Notificator<T>[] notificator)
        {
            welistener = new WeakEventListenerHolder();
            notificator.ForEach(n => NotificatorHelper.BindNotification(n, this, (o, e) => Notify(o, e)));
        }

        public event EventHandler<T> Notify = (o, e) => { };

        private WeakEventListenerHolder welistener;
        public WeakEventListenerHolder ListenerHolder
        {
            get { return welistener; }
        }
    }
}
