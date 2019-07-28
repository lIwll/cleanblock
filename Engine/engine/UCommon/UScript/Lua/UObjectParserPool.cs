using System;
using System.Collections.Generic;

using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
	public class UObjectParserPool
	{
        private static volatile UObjectParserPool
                        mInstance = new UObjectParserPool();
        private Dictionary< ULuaState, WeakReference >
                        mParsers = new Dictionary< ULuaState, WeakReference >();
        ULuaState       mLastState = default(ULuaState);
        UObjectParser   mLastParser = default(UObjectParser);

		public static UObjectParserPool Instance
		{
			get
			{
				return mInstance;
			}
		}
		
		public UObjectParserPool()
		{
		}
		
		public void Add (ULuaState L, UObjectParser parser)
		{
			mParsers.Add(L , new WeakReference(parser));			
		}

		public UObjectParser Find(ULuaState L)
		{
            if (mLastState == L)
                return mLastParser;

            if (mParsers.ContainsKey(L))
            {
                mLastState  = L;
                mLastParser = mParsers[L].Target as UObjectParser;

                return mLastParser;
            }

			ULuaState main = Utils.GetMainState(L);
            if (mParsers.ContainsKey(main))
            {
                mLastState  = L;
                mLastParser = mParsers[main].Target as UObjectParser;
                mParsers[L] = new WeakReference(mLastParser);

                return mLastParser;
            }
			
			return null;
		}
		
		public void Remove(ULuaState L)
		{
			if (!mParsers.ContainsKey(L))
				return;
			
            if (mLastState == L)
            {
                mLastState  = default(ULuaState);
                mLastParser = default(UObjectParser);
            }

            UObjectParser parser = mParsers[L].Target as UObjectParser;

            List< ULuaState > toberemove = new List< ULuaState >();

            foreach (var kv in mParsers)
            {
                if ((kv.Value.Target as UObjectParser) == parser)
                    toberemove.Add(kv.Key);
            }

			for (int i = 0; i < toberemove.Count; ++ i)
				mParsers.Remove(toberemove[i]);
        }
    }
}
