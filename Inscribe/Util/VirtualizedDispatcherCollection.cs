using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Livet;

namespace Inscribe.Util
{
    public class VirtualizedDispatcherCollection<T> : INotifyCollectionChanged, IList<T>, IList
    {
        public VirtualizedDispatcherCollection()
        {
            internalCollection = new List<T>();
        }

        public VirtualizedDispatcherCollection(IEnumerable<T> collection)
            : this()
        {
            internalCollection.AddRange(collection);
        }

        protected List<T> internalCollection;

        protected object syncRoot = new object();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                DispatcherHelper.BeginInvoke(() => handler(this, args));
            }
        }

        public int IndexOf(T item)
        {
            lock (syncRoot)
            {
                return internalCollection.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (syncRoot)
            {
                internalCollection.Insert(index, item);
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }

        public void RemoveAt(int index)
        {
            lock (syncRoot)
            {
                var item = internalCollection[index];
                internalCollection.RemoveAt(index);
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }
        }

        public T this[int index]
        {
            get
            {
                lock (syncRoot)
                {
                    return internalCollection[index];
                }
            }
            set
            {
                lock (syncRoot)
                {
                    internalCollection[index] = value;
                }
            }
        }

        public void Add(T item)
        {
            lock (syncRoot)
            {
                internalCollection.Add(item);
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, internalCollection.Count - 1));
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                internalCollection.Clear();
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public bool Contains(T item)
        {
            lock (syncRoot)
            {
                return internalCollection.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (syncRoot)
            {
                internalCollection.CopyTo(array, arrayIndex);
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public int Count
        {
            get
            {
                lock (syncRoot)
                {
                    return internalCollection.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            lock (syncRoot)
            {
                var index = internalCollection.IndexOf(item);
                if (index == -1)
                    return false;
                if (!internalCollection.Remove(item))
                    throw new InvalidOperationException("Multi-threaded access?");
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                return true;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return internalCollection.GetEnumerator();
        }

        /// <summary>
        /// WARNING: THIS METHOD RETURNS EMPTY DATA
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // TRICK FOR FASTER LOADING
            yield break;
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            return this.Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            this.Insert(index, (T)value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            this.Remove((T)value);
        }

        object IList.this[int index]
        {
            get
            {
                return internalCollection[index];
            }
            set
            {

                internalCollection[index] = (T)value;
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return syncRoot; }
        }
    }
}