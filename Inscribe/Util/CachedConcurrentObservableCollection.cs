using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Livet;

namespace Inscribe.Util
{
    public class CachedConcurrentObservableCollection<T>
        : INotifyCollectionChanged, ICollection<T>
    {
        #region Internal storage

        private object arraySyncRoot = new object();

        private T[] BehindArray;

        private object syncRoot = new object();

        private LinkedList<T> BehindCollection;

        #endregion

        public CachedConcurrentObservableCollection()
        {
            this.BehindArray = new T[0];
            this.BehindCollection = new LinkedList<T>();
        }

        /// <summary>
        /// コレクションの変更をキャッシュに反映します。<para />
        /// ディスパッチャスレッドから実行した場合、ノンブロック実行します。
        /// </summary>
        /// <param name="invalidateCollection">コレクションが変更されたことを通知するか</param>
        public void Commit(bool invalidateCollection = false)
        {
            if (DispatcherHelper.UIDispatcher.CheckAccess())
            {
                Task.Factory.StartNew(() => Commit(invalidateCollection));
                return;
            }
            T[] array;
            lock (syncRoot)
            {
                array = BehindCollection.ToArray();
            }
            Interlocked.Exchange(ref BehindArray, array);
            if (invalidateCollection)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Add(T item)
        {
            lock (syncRoot)
            {
                this.BehindCollection.AddLast(item);
            }
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                this.BehindCollection.Clear();
            }
            this.Commit(true);
        }

        public bool Contains(T item)
        {
            lock (syncRoot)
            {
                return this.BehindCollection.Contains(item);
            }
        }

        public bool AddTopSingle(T item)
        {
            bool add = true;
            lock (syncRoot)
            {
                if (!this.BehindCollection.Contains(item))
                    this.BehindCollection.AddFirst(item);
                else
                    add = false;
            }
            if (add)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, 0));
            return add;
        }


        public bool AddLastSingle(T item)
        {
            bool add = true;
            int addPoint = 0;
            lock (syncRoot)
            {
                addPoint = this.BehindCollection.Count;
                if (!this.BehindCollection.Contains(item))
                    this.BehindCollection.AddLast(item);
                else
                    add = false;
            }
            if (add)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, addPoint));
            return add;
        }

        public bool AddOrderedSingle<TKey>(T item, bool ascend, Func<T, TKey> keySelector)
             where TKey : IComparable
        {
            int addPoint = 0;
            lock (syncRoot)
            {
                if (!this.BehindCollection.Contains(item))
                {
                    var ikey = keySelector(item);
                    T node;
                    if (ascend)
                        node = this.BehindCollection.FirstOrDefault(i => keySelector(i).CompareTo(ikey) > 0);
                    else
                        node = this.BehindCollection.LastOrDefault(i => keySelector(i).CompareTo(ikey) > 0);
                    if (node == null)
                    {
                        if (ascend)
                        {
                            addPoint = this.BehindCollection.Count;
                            this.BehindCollection.AddLast(item);
                        }
                        else
                        {
                            addPoint = 0;
                            this.BehindCollection.AddFirst(item);
                        }
                    }
                    else
                    {
                        if (ascend)
                        {
                            int count = 0;
                            for (LinkedListNode<T> lnode = this.BehindCollection.First; lnode != null; lnode = lnode.Next)
                            {
                                if (lnode.Value.Equals(node))
                                {
                                    this.BehindCollection.AddBefore(lnode, item);
                                    break;
                                }
                                count++;
                            }
                            if (count == this.BehindCollection.Count)
                                throw new InvalidOperationException("Addition out of range.");
                            else
                                addPoint = count;
                        }
                        else
                        {
                            int count = this.Count;
                            for (LinkedListNode<T> lnode = this.BehindCollection.Last; lnode != null; lnode = lnode.Previous)
                            {
                                if (lnode.Value.Equals(node))
                                {
                                    this.BehindCollection.AddAfter(lnode, item);
                                    break;
                                }
                                count--;
                            }
                            if (count == 0)
                                throw new InvalidOperationException("Addition out of range.");
                            else
                                addPoint = count;
                        }
                    }
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, addPoint));
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool AddVolatile(T item)
        {
            lock (syncRoot)
            {
                if (!this.BehindCollection.Contains(item))
                {
                    this.BehindCollection.AddFirst(item);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Sort<TKey>(bool ascend, Func<T, TKey> keySelector)
        {
            lock (syncRoot)
            {
                var array = this.BehindCollection.ToArray();
                this.BehindCollection.Clear();
                if (ascend)
                    array.OrderBy(keySelector).ForEach(i => this.BehindCollection.AddLast(i));
                else
                    array.OrderByDescending(keySelector).ForEach(i => this.BehindCollection.AddLast(i));
            }
            this.Commit(true);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (arraySyncRoot)
            {
                this.BehindArray.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get { return this.BehindCollection.Count; }
        }

        public bool Remove(T item)
        {
            int idx;
            bool ret;
            lock (syncRoot)
            {
                idx = Array.IndexOf(this.BehindCollection.ToArray(), item);
                ret = this.BehindCollection.Remove(item);
            }
            if (idx >= 0)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, idx));
            return ret;
        }

        public T[] ToArrayVolatile()
        {
            lock (syncRoot)
            {
                return this.BehindCollection.ToArray();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.BehindArray.Cast<T>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = this.CollectionChanged;
            if (handler != null)
                DispatcherHelper.BeginInvoke(() =>
                    {
                        try
                        {
                            handler(this, e);
                        }
                        catch { }
                    });
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }
    }
}
