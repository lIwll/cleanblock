using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime;
using System.Runtime.InteropServices;

namespace UEngine
{
    public sealed class KeyedPriorityQueueHeadChangedEventArgs< T > : EventArgs where T : class
    {
        private T mNewFirstElement;
        private T mOldFirstElement;

        public KeyedPriorityQueueHeadChangedEventArgs(T oldFirstElement, T newFirstElement)
        {
            mOldFirstElement = oldFirstElement;
            mNewFirstElement = newFirstElement;
        }

        public T NewFirstElement
        {
            get
            {
                return mNewFirstElement;
            }
        }

        public T OldFirstElement
        {
            get
            {
                return mOldFirstElement;
            }
        }
    }

    [Serializable]
    public class UKeyedPriorityQueue< K, V, P > where V : class
    {
        private int                             mSize;
        private List< UHeapNode< K, V, P > >    mHeap;
        private UHeapNode< K, V, P >            mPlaceHolder;
        private Comparer< P >                   mPriorityComparer;

        public event EventHandler< KeyedPriorityQueueHeadChangedEventArgs< V > > FirstElementChanged;

        public UKeyedPriorityQueue()
        {
            mSize = 0;

            mPlaceHolder = new UHeapNode< K, V, P >();

            mPriorityComparer = Comparer< P >.Default;

            mHeap = new List< UHeapNode< K, V, P > >();
            mHeap.Add(new UHeapNode< K, V, P >());
        }

        public void Clear()
        {
            mHeap.Clear();

            mSize = 0;
        }

        public V Dequeue()
        {
            V local = (mSize < 1) ? default(V) : DequeueImpl();
            V newHead = (mSize < 1) ? default(V) : mHeap[1].mValue;
            RaiseHeadChangedEvent(default(V), newHead);

            return local;
        }

        private V DequeueImpl()
        {
            V local = mHeap[1].mValue;

            mHeap[1] = mHeap[mSize];
            mHeap[mSize --] = mPlaceHolder;
            Heapify(1);

            return local;
        }

        public void Enqueue(K key, V value, P priority)
        {
            V local = (mSize > 0) ? mHeap[1].mValue : default(V);

            int num = ++ mSize;
            int num2 = num / 2;
            if (num == mHeap.Count)
                mHeap.Add(mPlaceHolder);

            while ((num > 1) && IsHigher(priority, mHeap[num2].mPriority))
            {
                mHeap[num] = mHeap[num2];
                num = num2;
                num2 = num / 2;
            }
            mHeap[num] = new UHeapNode< K, V, P >(key, value, priority);

            V newHead = mHeap[1].mValue;
            if (!newHead.Equals(local))
                RaiseHeadChangedEvent(local, newHead);
        }

        public V FindByPriority(P priority, Predicate< V > match)
        {
            if (mSize >= 1)
                return Search(priority, 1, match);

            return default(V);
        }

        private void Heapify(int i)
        {
            int num = 2 * i;
            int num2 = num + 1;
            int j = i;

            if ((num <= mSize) && IsHigher(mHeap[num].mPriority, mHeap[i].mPriority))
                j = num;

            if ((num2 <= mSize) && IsHigher(mHeap[num2].mPriority, mHeap[j].mPriority))
                j = num2;

            if (j != i)
            {
                Swap(i, j);

                Heapify(j);
            }
        }

        protected virtual bool IsHigher(P p1, P p2)
        {
            return (mPriorityComparer.Compare(p1, p2) < 1);
        }

        public V Peek()
        {
            if (mSize >= 1)
                return mHeap[1].mValue;

            return default(V);
        }

        private void RaiseHeadChangedEvent(V oldHead, V newHead)
        {
            if (oldHead != newHead)
            {
                EventHandler< KeyedPriorityQueueHeadChangedEventArgs< V > > firstElementChanged = FirstElementChanged;
                if (firstElementChanged != null)
                    firstElementChanged(this, new KeyedPriorityQueueHeadChangedEventArgs< V >(oldHead, newHead));
            }
        }

        public V Remove(K key)
        {
            if (mSize >= 1)
            {
                V oldHead = mHeap[1].mValue;

                for (int i = 1; i <= mSize; i ++)
                {
                    if (mHeap[i].mKey.Equals(key))
                    {
                        V local2 = mHeap[i].mValue;

                        Swap(i, mSize);
                        mHeap[mSize --] = mPlaceHolder;
                        Heapify(i);

                        V local3 = mHeap[1].mValue;
                        if (!oldHead.Equals(local3))
                            RaiseHeadChangedEvent(oldHead, local3);

                        return local2;
                    }
                }
            }

            return default(V);
        }

        private V Search(P priority, int i, Predicate< V > match)
        {
            V local = default(V);

            if (IsHigher(mHeap[i].mPriority, priority))
            {
                if (match(mHeap[i].mValue))
                    local = mHeap[i].mValue;

                int num = 2 * i;
                int num2 = num + 1;
                if ((local == null) && (num <= mSize))
                    local = Search(priority, num, match);

                if ((local == null) && (num2 <= mSize))
                    local = this.Search(priority, num2, match);
            }

            return local;
        }

        private void Swap(int i, int j)
        {
            UHeapNode< K, V, P > node = mHeap[i];
            mHeap[i] = mHeap[j];
            mHeap[j] = node;
        }

        public int Count
        {
            get
            {
                return mSize;
            }
        }

        public ReadOnlyCollection< K > Keys
        {
            get
            {
                List< K > list = new List< K >();
                for (int i = 1; i <= mSize; i ++)
                    list.Add(mHeap[i].mKey);

                return new ReadOnlyCollection<K>(list);
            }
        }

        public ReadOnlyCollection< V > Values
        {
            get
            {
                List< V > list = new List< V >();
                for (int i = 1; i <= mSize; i ++)
                    list.Add(mHeap[i].mValue);

                return new ReadOnlyCollection<V>(list);
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        private struct UHeapNode< KK, VV, PP >
        {
            public KK mKey;
            public VV mValue;
            public PP mPriority;

            public UHeapNode(KK key, VV value, PP priority)
            {
                mKey = key;
                mValue = value;
                mPriority = priority;
            }
        }
    }
}
