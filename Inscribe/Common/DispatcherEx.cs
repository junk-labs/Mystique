namespace System.Windows.Threading
{
    public static partial class DispatcherEx
    {
        public static void Invoke(this Dispatcher dispatcher, Action action)
        {
            dispatcher.Invoke(action, null);
        }

        public static void Invoke<T>(this Dispatcher dispatcher, Action<T> action, T param)
        {
            dispatcher.Invoke(action, param);
        }

        public static T Invoke<T>(this Dispatcher dispatcher, Func<T> func)
            where T : class
        {
            return dispatcher.Invoke(func, null) as T;
        }

        public static void BeginInvoke(this Dispatcher dispatcher, Action action)
        {
            dispatcher.BeginInvoke(action, null);
        }

        public static void BeginInvoke<T>(this Dispatcher dispatcher, Action<T> action, T param)
        {
            dispatcher.BeginInvoke(action, param);
        }

        public static void BeginInvoke<T>(this Dispatcher dispatcher, Func<T> func, Action<T> closure)
            where T : class
        {
            var dop = dispatcher.BeginInvoke(func, null);
            dop.Completed += (o, e) => closure(dop.Result as T);
        }

        public static void BeginInvoke(this Dispatcher dispatcher, Action action, DispatcherPriority priority)
        {
            dispatcher.BeginInvoke(action, priority, null);
        }

        public static void BeginInvoke<T>(this Dispatcher dispatcher, Action<T> action, T param, DispatcherPriority priority)
        {
            dispatcher.BeginInvoke(action, priority, param);
        }

        public static void BeginInvoke<T>(this Dispatcher dispatcher, Func<T> func, Action<T> closure, DispatcherPriority priority)
            where T : class
        {
            var dop = dispatcher.BeginInvoke(func, priority, null);
            dop.Completed += (o, e) => closure(dop.Result as T);
        }
    }
}
