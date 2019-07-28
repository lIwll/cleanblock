using System;

using UnityEngine;

namespace UEngine
{
	public class UMainTask
	{
		static UCommandBuffer< UTask > mCommands = new UCommandBuffer< UTask >(1024);
		//static URingBuffer< UTask > mCommands = new URingBuffer< UTask >(1024);

		public static void AddTask(UTask task)
		{
			mCommands.Write(task);
		}

		public static void DoTask()
		{
			float bt = Time.realtimeSinceStartup;

			UTask task = null;
			while (mCommands.Read(ref task))
			{
				task.DoTask();

				if (Time.realtimeSinceStartup - bt > USystemConfig.Instance.MainTaskLimit)
					break;
			}
		}
	}
}
