using System;
using System.Collections;
using System.Collections.Generic;

namespace UEngine
{
    public class UObjectPool
    {
        const int kListEnd = -1;
        const int kAlloced = -2;

        private SNode[] mList = new SNode[512];
        private int mFreelist = kListEnd;
        private int mCount = 0;

        struct SNode
        {
            public object
                mObj;
            public int
                mNext;

            public SNode(int next, object obj)
            {
                mObj    = obj;
                mNext   = next;
            }
        }

        public object this[int i]
        {
            get
            {
                if (i >= 0 && i < mCount)
                    return mList[i].mObj;

                return null;
            }
        }

        public void Clear()
        {
            mFreelist   = kListEnd;
            mCount      = 0;
            mList       = new SNode[512];
        }

        void ExtendCapacity()
        {
            SNode[] new_list = new SNode[mList.Length * 2];
            for (int i = 0; i < mList.Length; i ++)
                new_list[i] = mList[i];
            mList = new_list;
        }

        public int Add(object obj)
        {
            int index = kListEnd;

            if (mFreelist != kListEnd)
            {
                index = mFreelist;

                mList[index].mObj = obj;
                mFreelist = mList[index].mNext;
                mList[index].mNext = kAlloced;
            } else
            {
                if (mCount == mList.Length)
                    ExtendCapacity();

                index = mCount;
                mList[index] = new SNode(kAlloced, obj);
                mCount = index + 1;
            }

            return index;
        }

        public bool TryGetValue(int index, out object obj)
        {
            if (index >= 0 && index < mCount && mList[index].mNext == kAlloced)
            {
                obj = mList[index].mObj;

                return true;
            }

            obj = null;

            return false;
        }

        public object Get(int index)
        {
            if (index >= 0 && index < mCount)
                return mList[index].mObj;

            return null;
        }

        public object Remove(int index)
        {
            if (index >= 0 && index < mCount && mList[index].mNext == kAlloced)
            {
                object o = mList[index].mObj;
                mList[index].mObj = null;
                mList[index].mNext = mFreelist;
                mFreelist = index;

                return o;
            }

            return null;
        }

        public object Replace(int index, object o)
        {
            if (index >= 0 && index < mCount)
            {
                object obj = mList[index].mObj;
                mList[index].mObj = o;

                return obj;
            }

            return null;
        }

        public int Check(int check_pos, int max_check, Func< object, bool > checker, Dictionary< object, int > reverse_map)
        {
            if (mCount == 0)
                return 0;

            for (int i = 0; i < Math.Min(max_check, mCount); ++ i)
            {
                check_pos %= mCount;
                if (mList[check_pos].mNext == kAlloced && !Object.ReferenceEquals(mList[check_pos].mObj, null))
                {
                    if (!checker(mList[check_pos].mObj))
                    {
                        object obj = Replace(check_pos, null);

                        int obj_index;
                        if (reverse_map.TryGetValue(obj, out obj_index) && obj_index == check_pos)
                            reverse_map.Remove(obj);
                    }
                }

                ++ check_pos;
            }

            return check_pos %= mCount;
        }
    }
}
