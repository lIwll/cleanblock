using System;
using System.Threading;

using UnityEngine;

namespace UEngine
{
	public class ULoadThread : UThread
	{
#if USE_MEMORY_BUNDLE
		class ULoadedTask : UTask
		{
			ULoadTask mTask;
			Action< byte[], Action > mOnLoaaded;

			byte[] mData;

			public ULoadedTask()
			{ }

			public ULoadedTask(ULoadTask task, Action< byte[], Action > onLoaded, byte[] bytes)
			{
				mTask = task;
				mData = bytes;
				mOnLoaaded = onLoaded;
			}

			public override bool IsCancel()
			{
				if (null == mTask)
					return true;

				return mCancel || mTask.IsCancel();
			}

			public override void DoTask()
			{
				if (null != mOnLoaaded && !IsCancel())
					mOnLoaaded(mData, ()=>
					{
						mData = null;
					});
			}
		}
#else
		class ULoadedTask : UTask
		{
			ULoadTask mTask;
			Action< string, int, Action > mOnLoaaded;

			string mFilePath;
			int mFileOffset;

			public ULoadedTask()
			{ }

			public ULoadedTask(ULoadTask task, Action< string, int, Action > onLoaded, string filePath, int fileOffset)
			{
				mTask		= task;
				mFilePath	= filePath;
				mFileOffset = fileOffset;
				mOnLoaaded	= onLoaded;
			}

			public override bool IsCancel()
			{
				if (null == mTask)
					return true;

				return mCancel || mTask.IsCancel();
			}

			public override void DoTask()
			{
				if (null != mOnLoaaded && !IsCancel())
					mOnLoaaded(mFilePath, mFileOffset, () =>
					{
					});
			}
		}
#endif

		public class ULoadTask : UTask
		{
			string mFileName = "";

#if USE_MEMORY_BUNDLE
			Action< byte[], Action > mOnLoaded = null;
#else
			Action< string, int, Action > mOnLoaded = null;
#endif

			public ULoadTask()
			{
			}

#if USE_MEMORY_BUNDLE
			public ULoadTask(string fileName, Action< byte[], Action > onLoaded)
#else
			public ULoadTask(string fileName, Action< string, int, Action > onLoaded)
#endif
			{
				mFileName	= fileName;
				mOnLoaded	= onLoaded;
			}

			public override void DoTask()
			{
				if (!IsCancel())
				{
#if USE_MEMORY_BUNDLE
					byte[] bytes = UFileAccessor.ReadBinaryFile(mFileName);

					UMainTask.AddTask(new ULoadedTask(this, mOnLoaded, bytes));
#else
					string filePath; int fileOffset;
					if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
						UFileReaderProxy.QueryFile(mFileName, out filePath, out fileOffset);
					else
						UFileAccessor.QueryFile(mFileName, out filePath, out fileOffset);

					UMainTask.AddTask(new ULoadedTask(this, mOnLoaded, filePath, fileOffset));
#endif
				}
			}
		}

		static UCommandBuffer< ULoadTask > mCommands = new UCommandBuffer< ULoadTask >(1024);
		//static URingBuffer< ULoadTask > mCommands = new URingBuffer< ULoadTask >(1024);

		static ULoadThread msInstance = null;
		public static ULoadThread Instance
		{
			get
			{
				if (null == msInstance)
					msInstance = new ULoadThread();

				return msInstance;
			}
		}

		private ULoadThread()
		{
		}

#if USE_MEMORY_BUNDLE
		public ULoadTask AddLoadTask(string fileName, Action< byte[], Action > onLoaded)
#else
		public ULoadTask AddLoadTask(string fileName, Action< string, int, Action > onLoaded)
#endif
		{
			ULoadTask task = new ULoadTask(fileName, onLoaded);

			mCommands.Write(task);

			return task;
		}

		public string Print()
		{
			return string.Format("Total file load task {0}.", mCommands.Available);
		}

		protected override void Do()
		{
			ULoadTask task = null;
			if (mCommands.Read(ref task))
			{
				task.DoTask();
			} else
			{
				Thread.Sleep(50);
			}
		}
	}
}
