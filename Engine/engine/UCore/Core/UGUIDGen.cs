using System.Collections.Generic;

namespace UEngine
{
    public class UGUIDGen< T >
    {
        static int mID;
        static List< int > mIDs = new List< int >();

        static UGUIDGen()
        { }

        public static int NewID()
        {
            int id = mID;

            while (true)
            {
                if (!mIDs.Contains(id))
                {
                    mIDs.Add(id);

                    break;
                }

                id = ++ mID;
            }

            return id;
        }

        public static bool AddID(int id)
        {
            if (!mIDs.Contains(id))
            {
                mIDs.Add(id);

                return true;
            }

            return false;
        }

        public static bool RmvID(int id)
        {
            if (mIDs.Contains(id))
            {
                mIDs.Remove(id);

                return true;
            }

            return false;
        }

        public static bool Contains(int id)
        {
            return mIDs.Contains(id);
        }

        public static void Clear()
        {
            mID = 0;
            mIDs.Clear();
        }
    }
}