using System;
using System.Threading;

namespace UEngine
{
	public class UThread
	{
		private bool mIsRuning = false;

		public void Start()
		{
			Action act = () =>
			{
				mIsRuning = true;

				Run();
			};
			act.BeginInvoke(null, null);
		}

		public void Stop()
		{
			mIsRuning = false;
		}

		protected virtual void Do()
		{
			Thread.Sleep(1000);
		}

		private void Run()
		{
			while (mIsRuning)
			{
				Do();
			}
		}
	}
}
