using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using System.Collections.Specialized;

namespace Inscribe.Util
{
    public class VirtualizedDispatcherTimelineCollection : VirtualizedDispatcherCollection<TabDependentTweetViewModel>
    {
        public bool AddTopSingle(TabDependentTweetViewModel item)
        {
            bool add = true;
            lock (syncRoot)
            {
                if (!this.internalCollection.Contains(item))
                    this.internalCollection.Insert(0, item);
                else
                    add = false;
            }
            if (add)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, 0));
            return add;
        }

        public bool AddLastSingle(TabDependentTweetViewModel item)
        {
            bool add = true;
            int addPoint = 0;
            lock (syncRoot)
            {
                addPoint = internalCollection.Count;
                if (!this.internalCollection.Contains(item))
                    this.internalCollection.Add(item);
                else
                    add = false;
            }
            if (add)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, addPoint));
            return add;
        }

        public bool AddOrderedSingle<TKey>(TabDependentTweetViewModel item, bool ascend, Func<TabDependentTweetViewModel, TKey> keySelector)
             where TKey : IComparable
        {
            int addPoint = 0;
            lock (syncRoot)
            {
                if (!this.internalCollection.Contains(item))
                {
                    var ikey = keySelector(item);
                    T node;
                    if (ascend)
                        node = this.internalCollection.FirstOrDefault(i => keySelector(i).CompareTo(ikey) > 0);
                    else
                        node = this.internalCollection.LastOrDefault(i => keySelector(i).CompareTo(ikey) > 0);
                    if (node == null)
                    {
                        if (ascend)
                        {
                            addPoint = this.internalCollection.Count;
                            this.internalCollection.Add(item);
                        }
                        else
                        {
                            addPoint = 0;
                            this.internalCollection.Insert(0, item);
                        }
                    }
                    else
                    {
                        if (ascend)
                        {
                            int count = 0;
                            for (LinkedListNode<T> lnode = this.internalCollection[0]; lnode != null; lnode = lnode.Next)
                            {
                                if (lnode.Value.Equals(node))
                                {
                                    this.internalCollection.AddBefore(lnode, item);
                                    break;
                                }
                                count++;
                            }
                            if (count == this.internalCollection.Count)
                                throw new InvalidOperationException("Addition out of range.");
                            else
                                addPoint = count;
                        }
                        else
                        {
                            int count = this.Count;
                            for (LinkedListNode<T> lnode = this.internalCollection.Last; lnode != null; lnode = lnode.Previous)
                            {
                                if (lnode.Value.Equals(node))
                                {
                                    this.internalCollection.AddAfter(lnode, item);
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

        public void AddRangeVolatile(IEnumerable<T> item)
        {
            lock (syncRoot)
            {
                item.ForEach(i => this.internalCollection.AddFirst(i));
            }
        }

        public bool AddVolatile(T item)
        {
            lock (syncRoot)
            {
                if (!this.internalCollection.Contains(item))
                {
                    this.internalCollection.AddFirst(item);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void SortSingle<TKey>(bool ascend, Func<T, TKey> keySelector)
        {
            lock (syncRoot)
            {
                var array = this.internalCollection.ToArray();
                this.internalCollection.Clear();
                if (ascend)
                    array.OrderBy(keySelector).Distinct().ForEach(i => this.internalCollection.AddLast(i));
                else
                    array.OrderByDescending(keySelector).Distinct().ForEach(i => this.internalCollection.AddLast(i));
            }
            this.Commit(true);
        }

        public void Sort<TKey>(bool ascend, Func<T, TKey> keySelector)
        {
            lock (syncRoot)
            {
                var array = this.internalCollection.ToArray();
                this.internalCollection.Clear();
                if (ascend)
                    array.OrderBy(keySelector).ForEach(i => this.internalCollection.AddLast(i));
                else
                    array.OrderByDescending(keySelector).ForEach(i => this.internalCollection.AddLast(i));
            }
            this.Commit(true);
        }


        public TabDependentTweetViewModel[] ToArrayVolatile()
        {
            lock (syncRoot)
            {
                return this.internalCollection.ToArray();
            }
        }
    }
}
