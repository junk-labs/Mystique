using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Livet;

namespace Inscribe.Data
{
    [Serializable]
    public class SafeObservable<T> : ReaderWriterLockBase, IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanging;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        readonly List<T> internalList = new List<T>();
        readonly Dispatcher dispatcher = DispatcherHelper.UIDispatcher ?? Application.Current.Dispatcher ?? Dispatcher.CurrentDispatcher;

        public bool NotifyChanges
        {
            get;
            private set;
        }

        public SafeObservable()
        {
            this.NotifyChanges = true;
        }

        public SafeObservable(IEnumerable<T> collection)
            : this()
        {
            AddRange(collection);
        }

        bool RequireInvoke
        {
            get
            {
                return Thread.CurrentThread != dispatcher.Thread;
            }
        }

        public int IndexOf(T item)
        {
            using (AcquireReaderLock(readerWriterLock))
                return internalList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (this.RequireInvoke)
                dispatcher.Invoke((Action<int, T>)Insert, index, item);
            else
            {
                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));

                using (AcquireWriterLock(readerWriterLock))
                    internalList.Insert(index, item);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }

        public void RemoveAt(int index)
        {
            if (this.RequireInvoke)
                dispatcher.Invoke((Action<int>)RemoveAt, index);
            else
            {
                if (index >= internalList.Count)
                    return;

                var value = internalList[index];

                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value, index));

                using (AcquireWriterLock(readerWriterLock))
                    internalList.RemoveAt(index);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value, index));
            }
        }

        public IEnumerable<T> RemoveWhere(Func<T, bool> predicate)
        {
            foreach (var i in this.Where(predicate).ToArray())
            {
                this.Remove(i);
                yield return i;
            }
        }

        public T this[int index]
        {
            get
            {
                using (AcquireReaderLock(readerWriterLock))
                    return index >= this.Count ? default(T) : internalList[index];
            }
            set
            {
                if (this.RequireInvoke)
                    dispatcher.Invoke((Action)(() => this[index] = value));
                else
                {
                    OnCollectionChanging(new NotifyCollectionChangedEventArgs
                    (
                        NotifyCollectionChangedAction.Replace,
                        internalList[index],
                        value,
                        index
                    ));

                    using (AcquireWriterLock(readerWriterLock))
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs
                        (
                            NotifyCollectionChangedAction.Replace,
                            internalList[index],
                            internalList[index] = value,
                            index
                        ));
                }
            }
        }

        public void Add(T item)
        {
            if (this.RequireInvoke)
                dispatcher.Invoke((Action<T>)Add, item);
            else
            {
                var addidx = internalList.Count;
                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, addidx));

                using (AcquireWriterLock(readerWriterLock))
                    internalList.Add(item);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, addidx));
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (this.RequireInvoke)
                dispatcher.Invoke((Action<IEnumerable<T>>)AddRange, collection);
            else
            {
                foreach (var item in collection)
                    this.Add(item);
            }
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            var idx = 0;

            foreach (var i in collection)
                this.Insert(index + idx++, i);
        }

        public void Move(int oldIndex, int newIndex)
        {
            if (this.RequireInvoke)
                dispatcher.Invoke((Action<int, int>)Move, oldIndex, newIndex);
            else
            {
                if (oldIndex < 0 || newIndex < 0 || oldIndex >= internalList.Count || newIndex >= internalList.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }
                var item = internalList[oldIndex];
                internalList.RemoveAt(oldIndex);
                internalList.Insert(newIndex, item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
            }
        }

        public void Clear()
        {
            if (this.RequireInvoke)
                dispatcher.Invoke((Action)Clear);
            else
            {
                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                using (AcquireWriterLock(readerWriterLock))
                    internalList.Clear();

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// コレクションの要素を置き換えます。<para />
        /// 置き換える前の要素を返します。
        /// </summary>
        public IEnumerable<T> ReplaceCollection(IEnumerable<T> collection)
        {
            if (this.RequireInvoke)
                return dispatcher.Invoke((Func<IEnumerable<T>, IEnumerable<T>>)ReplaceCollection, collection) as IEnumerable<T>;
            else
            {
                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                T[] prev = null;
                using (AcquireWriterLock(readerWriterLock))
                {
                    prev = internalList.ToArray();
                    internalList.Clear();
                    internalList.AddRange(collection);
                }
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return prev;
            }
        }

        public bool Contains(T item)
        {
            using (AcquireReaderLock(readerWriterLock))
                return internalList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            using (AcquireReaderLock(readerWriterLock))
                internalList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                using (AcquireReaderLock(readerWriterLock))
                    return internalList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(T item)
        {
            if (this.RequireInvoke)
                return (bool)dispatcher.Invoke((Func<T, bool>)Remove, item);
            else
            {
                var index = internalList.IndexOf(item);

                if (index == -1)
                    return false;

                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));

                using (AcquireWriterLock(readerWriterLock))
                    internalList.Remove(item);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));

                return true;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var i in internalList.ToArray())
                yield return i;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual void OnCollectionChanging(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanging != null && this.NotifyChanges)
                CollectionChanging(this, e);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null && this.NotifyChanges)
                CollectionChanged(this, e);

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item"));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null && this.NotifyChanges)
                PropertyChanged(this, e);
        }

        int IList.Add(object value)
        {
            this.Add((T)value);

            return this.IndexOf((T)value);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (T)value);
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        void IList.Remove(object value)
        {
            this.Remove((T)value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (T)value;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            using (AcquireReaderLock(readerWriterLock))
                for (int i = index; i < Math.Min(array.Length - index, this.Count); i++)
                    ((object[])array)[i] = this[i - index];
        }

        int ICollection.Count
        {
            get
            {
                return this.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return readerWriterLock;
            }
        }

        public IDisposable SusupendNotification(bool notifyReset)
        {
            return SusupendNotification(notifyReset, false);
        }

        public IDisposable SusupendNotification(bool notifyReset, bool checkRealChanges)
        {
            var cache = checkRealChanges ? this.ToArray() : null;

            return FinallyBlock.Create(this.NotifyChanges = false, _ =>
            {
                this.NotifyChanges = true;

                if (notifyReset && (!checkRealChanges || !this.SequenceEqual(cache)))
                    dispatcher.Invoke((Action<NotifyCollectionChangedEventArgs>)OnCollectionChanged,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }
    }
}
