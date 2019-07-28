using System;

namespace UEngine
{
	public class UTask
	{
		protected bool mCancel = false;

		public virtual void Cancel()
		{
			mCancel = true;
		}

		public virtual bool IsCancel()
		{
			return mCancel;
		}

		public virtual void DoTask()
		{
		}
	}
}
