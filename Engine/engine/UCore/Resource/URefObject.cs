using System;

namespace UEngine
{
	public class URefObject
	{
		protected int mRefCnt = 0;
		public int RefCnt
		{ get { return mRefCnt; } }

		public URefObject()
		{
			mRefCnt = 0;
		}

		public virtual int AddRef()
		{
			mRefCnt ++;

			return mRefCnt;
		}

		public virtual int Release()
		{
			int refCnt = -- mRefCnt;
			if (refCnt == 0)
			{ }

			if (refCnt < 0)
				ULogger.Warn("Release refCnt = {0}", refCnt);

			return refCnt;
		}
	}
}
