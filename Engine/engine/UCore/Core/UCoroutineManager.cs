using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace UEngine
{
	public class UCoroutineManager
	{
		private static LinkedList< IEnumerator > mCoroutineTasks = new LinkedList< IEnumerator >();

		private static IEnumerator mCurrentWork = null;
		private static IEnumerator mCurrentTask = null;

		public static void AddTask(IEnumerator task)
		{
			mCoroutineTasks.AddLast(task);
		}

		public static bool RmvTask(IEnumerator task)
		{
			if (mCurrentTask != task)
				return mCoroutineTasks.Remove(task);

			return false;
		}

		public static void Update()
		{
#if xxx
			int tasks = 0;
			while (mCoroutineTasks.Count > 50)
			{
				DoTask();

				if (++ tasks > USystemConfig.Instance.SynCreateRequestCount)
					break;
			}
#endif

#if xxx
			if (mCoroutineQueue.Count > 20)
				DoTask();

			if (mCoroutineQueue.Count > 10)
				DoTask();
#endif

			DoTask();
		}

		public static bool IsRunningTask(IEnumerator task)
		{
			return mCurrentTask == task;
		}

		public static void WaitEndOfCurrentTask()
		{
			while (!DoTask())
			{ }
		}

		public static void WaitEndOfTask(IEnumerator task)
		{
			if (null == task)
				return;

			bool finish = !task.MoveNext();
			while (!finish)
			{
				if (task.Current is IEnumerator)
					task = (task.Current as IEnumerator);

				finish = !task.MoveNext();
			}
		}

		public static string Print()
		{
			return string.Format("Total asset create task {0}.", mCoroutineTasks.Count);
		}

		static bool DoTask()
		{
			if (mCurrentWork == null && mCoroutineTasks.Count == 0)
				return true;

			if (mCurrentWork == null)
			{
				var node = mCoroutineTasks.First;

				mCurrentTask = node.Value as IEnumerator;
				mCurrentWork = mCurrentTask;

				mCoroutineTasks.RemoveFirst();
			}

			while (true)
			{
				bool finish = !mCurrentWork.MoveNext();
				if (finish)
				{
					mCurrentWork = null;
					mCurrentTask = null;

					return true;
				} else if (mCurrentTask.Current == null)
				{
					break;
				}
			}

			return false;
		}
	}
}