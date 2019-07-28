using System;
using System.Diagnostics;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Profiling;

namespace UEngine
{
	class UProfileNode
	{
		protected string mName;

		protected long mTime;
		protected bool mLoop;

		protected UProfileNode mParent = null;
		public UProfileNode Parent
		{
			get { return mParent; }
		}

		public int mCallTimes = 0;

		int mLoopSampleIndex = 0;

		List< UProfileNode > mChilds = new List< UProfileNode >();

		Stack< UProfileNode > mSibling = new Stack< UProfileNode >();

		Stopwatch mStopWatch = new Stopwatch();

		Stopwatch mLoopStopWatch = new Stopwatch();

		public UProfileNode(string name, bool loop, UProfileNode parent)
		{
			mName		= name;
			mTime		= 0;
			mLoop		= loop;
			mParent		= parent;
			mCallTimes	= 0;

			mStopWatch.Start();
		}

		public UProfileNode(string name, bool loop, long time, UProfileNode parent)
		{
			mName		= name;
			mTime		= time;
			mLoop		= loop;
			mParent		= parent;
			mCallTimes	= 0;

			mStopWatch.Start();
		}

		public void Reset()
		{
			mTime = 0;

			mChilds.Clear();

			mStopWatch.Reset();
			mStopWatch.Start();
		}

		public UProfileNode Push(string name, bool loop = false)
		{
			UProfileNode node = null;

			if (mLoop)
			{
				if (mLoopSampleIndex >= mChilds.Count)
				{
					node = new UProfileNode(name, mLoop || loop, this);

					mChilds.Add(node);
				} else
				{
					node = mChilds[mLoopSampleIndex];
					node.mStopWatch.Start();
				}

				mLoopSampleIndex ++;
			} else
			{
				node = new UProfileNode(name, loop, this);

				mChilds.Add(node);
			}

			return node;
		}

		public void Pop()
		{
			mTime = mStopWatch.ElapsedMilliseconds;

			mStopWatch.Stop();
		}

		public UProfileNode PushCall(string name, UProfileNode sibling)
		{
			UProfileNode node = mChilds.Find((v) =>
			{
				return v.mName.Equals(name);
			});

			if (null == node)
			{
				node = new UProfileNode(name, false, this);

				mChilds.Add(node);
			} else
			{
				if (node.mSibling.Count == 0)
					node.mStopWatch.Start();
			}

			node.mCallTimes ++;
			node.mSibling.Push(sibling);

			return node;
		}

		public UProfileNode PopCall()
		{
			if (mSibling.Count > 0)
			{
				mTime = mStopWatch.ElapsedMilliseconds;

				var sibling = mSibling.Pop();
				if (mSibling.Count == 0)
					mStopWatch.Stop();

				return sibling;
			}

			return null;
		}

		public void NewLoopSample(int idx)
		{
			if (mLoop)
			{
				mLoopSampleIndex = 0;

				for (int i = 0; i < mChilds.Count; ++ i)
					mChilds[i].NewLoopSample(idx);
			}
		}

		public string Dump(int layer)
		{
			if (mTime <= 1)
				return string.Empty;

			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < layer; ++ i)
				builder.Append("\t");

			builder.AppendFormat("[{0}]: Total Use Time({1})", mName, mTime);
			if (mCallTimes > 0)
				builder.AppendFormat(", Total Call Times({0})\n", mCallTimes);
			else
				builder.AppendFormat("\n");

			foreach (var node in mChilds)
				builder.Append(node.Dump(layer + 1));

			return builder.ToString();
		}

		public long GetTimeStamp()
		{
			return mTime;
		}
	}

	public class UProfile
	{
		float mTime			= 0f;
		float mStartTime	= 0f;
		float mTimeout		= 0f;
		float mTotalTime	= 0f;
		bool mStarted		= false;

		struct SNode
		{
			public float mTime;
			public string mDesc;
		}
		List< SNode > mProfiles = new List< SNode >();

		static Dictionary< string, float[] > mLoadProfile = new Dictionary< string, float[] >();

		static UProfileNode msRootNode = null;

		static UProfileNode msCallRoot = null;
		static UProfileNode msCallNode = null;

		static UProfileNode msCurrentNode = null;

		public UProfile(float timeout)
		{
			mStarted	= false;
			mTime		= Time.realtimeSinceStartup;
			mStartTime	= Time.realtimeSinceStartup;
			mTimeout	= timeout;
			mProfiles.Clear();
		}

		static UProfile()
		{
			msRootNode		= new UProfileNode("__ROOT__", false, null);
			msCallRoot		= msRootNode.Push("__CALL__");
			msCallNode		= null;
			msCurrentNode	= msRootNode;
		}

		[Conditional("ENABLE_PROFILER")]
		public void Start()
		{
			if (USystemConfig.Instance.EnableProfile)
			{
				mStarted	= true;
				mTime		= Time.realtimeSinceStartup;
				mStartTime	= mTime;
				mProfiles.Clear();
			}
		}

		[Conditional("ENABLE_PROFILER")]
		public void Profile(string format, params object[] args)
		{
			if (mStarted)
			{
				string desc = string.Format(string.Format("Frame[{0:D16}]: ", Time.frameCount, UCore.CurrentFrame) + format, args);

				mProfiles.Add(new SNode() { mDesc = desc, mTime = Time.realtimeSinceStartup - mTime });

				mTime = Time.realtimeSinceStartup;
			}
		}

		[Conditional("ENABLE_PROFILER")]
		public void IsFinish(ref bool finished)
		{

			if (mStarted)
			{
				mStarted = false;

				mTotalTime = Time.realtimeSinceStartup - mStartTime;

				finished = (mTotalTime >= mTimeout);
			}
		}

		[Conditional("ENABLE_PROFILER")]
		public void GetInfo(ref string info)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("profile start-------------------------------------------\n");
			sb.Append(string.Format("profile total time: {0}\n", mTotalTime));
			for (int i = 0; i < mProfiles.Count; ++ i)
			{
				SNode node = mProfiles[i];

				sb.Append(string.Format("\tprofile item: {0,-30}, time consuming: {1,-8:#,###00.000000},  percentage: {2:P4}\n", node.mDesc, node.mTime, node.mTime / mTotalTime));
			}
			sb.Append("profile finish-------------------------------------------\n");

			info = sb.ToString();
		}

		[Conditional("ENABLE_PROFILER")]
		public void IsStarted(ref bool started)
		{
			started = mStarted;
		}

		[Conditional("ENABLE_PROFILER")]
		public void GetTimeout(ref float timeout)
		{
			timeout = mTimeout;
		}

		[Conditional("ENABLE_PROFILER")]
		public void GetTotalTime(ref float totaltime)
		{
			totaltime = mTotalTime;
		}

		[Conditional("ENABLE_PROFILER")]
		public static void StartSample()
		{
			msRootNode.Reset();
			msCallRoot		= msRootNode.Push("__CALL__");
			msCallNode		= null;
			msCurrentNode	= msRootNode;
		}

		[Conditional("ENABLE_PROFILER")]
		public static void StopSample()
		{
			msCallRoot.Pop();

			while (null != msCurrentNode)
			{
				msCurrentNode.Pop();

				msCurrentNode = msCurrentNode.Parent;
			}
			msCurrentNode = msRootNode;
		}

		[Conditional("ENABLE_PROFILER")]
		public static void BeginCallSample(string name)
		{
			if (null != msCallRoot)
				msCallNode = msCallRoot.PushCall(name, msCallNode);
		}

		[Conditional("ENABLE_PROFILER")]
		public static void BeginCallSample(string formatName, params object[] args)
		{
			var name = string.Format(formatName, args);

			if (null != msCallRoot)
				msCallNode = msCallRoot.PushCall(name, msCallNode);
		}

		[Conditional("ENABLE_PROFILER")]
		public static void EndCallSample()
		{
			if (null != msCallNode)
				msCallNode = msCallNode.PopCall();
		}

		[Conditional("ENABLE_PROFILER")]
		public static void BeginSample(string name)
		{
			msCurrentNode = msCurrentNode.Push(name);

			Profiler.BeginSample(name);
		}

		[Conditional("ENABLE_PROFILER")]
		public static void BeginSample(string formatName, params object[] args)
		{
			var name = string.Format(formatName, args);

			msCurrentNode = msCurrentNode.Push(name);

			Profiler.BeginSample(name);
		}

		[Conditional("ENABLE_PROFILER")]
		public static void EndSample()
		{
			msCurrentNode.Pop();

			msCurrentNode = msCurrentNode.Parent;

			Profiler.EndSample();
		}

		[Conditional("ENABLE_PROFILER")]
		public static void NewLoopSample(int idx)
		{
			if (null != msCurrentNode)
				msCurrentNode.NewLoopSample(idx);
		}

		[Conditional("ENABLE_PROFILER")]
		public static void BeginLoopSample(string name)
		{
			msCurrentNode = msCurrentNode.Push(name, true);
		}

		[Conditional("ENABLE_PROFILER")]
		public static void BeginLoopSample(string formatName, params object[] args)
		{
			var name = string.Format(formatName, args);

			msCurrentNode = msCurrentNode.Push(name, true);
		}

		[Conditional("ENABLE_PROFILER")]
		public static void EndLoopSample()
		{
			msCurrentNode.Pop();

			msCurrentNode = msCurrentNode.Parent;
		}

		[Conditional("ENABLE_PROFILER")]
		public static void Dump(string fileName, long timeout, Action< string > dump = null)
		{
			if (msCurrentNode.GetTimeStamp() >= timeout)
			{
				var info = msCurrentNode.Dump(0);
				if (string.IsNullOrEmpty(info))
					return;

				if (!string.IsNullOrEmpty(fileName))
				{
					var path = UFileAccessor.GetPath(fileName);
					if (!UFileAccessor.IsFileExist(fileName))
						UCoreUtil.CreateFile(path, null);

					UCoreUtil.AppendFile(path, System.Text.Encoding.Default.GetBytes(info));
				}

				if (null != dump)
					dump(info);
			}
		}

		[Conditional("ENABLE_PROFILER")]
		public static void ResetDump(string fileName)
		{
			if (!string.IsNullOrEmpty(fileName))
				UCoreUtil.CreateFile(UFileAccessor.GetPath(fileName), null);
		}

		static string[] mLoadStepName = new string[] { "WaitDepend", "LoadFile", "LoadAsset", "ProcessNavMesh", "ProcessShader", "ProcessMaterial" };

		[Conditional("ENABLE_PROFILER"), Conditional("ENABLE_LOAD_PROFILER")]
		public static void AddLoadProfile(string name, int step, float consume)
		{
			if (!mLoadProfile.ContainsKey(name))
			{
				var steps = new float[6];
					steps[0] = 0f;
					steps[1] = 0f;
					steps[2] = 0f;
					steps[3] = 0f;
					steps[4] = 0f;
					steps[5] = 0f;

				mLoadProfile[name] = steps;
			}

			mLoadProfile[name][step] = consume;
		}

		[Conditional("ENABLE_PROFILER"), Conditional("ENABLE_LOAD_PROFILER")]
		public static void ResetLoadProfile()
		{
			mLoadProfile.Clear();
		}

		[Conditional("ENABLE_PROFILER"), Conditional("ENABLE_LOAD_PROFILER")]
		public static void CollectLoadProfile(ref string info)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("LoadProfile:\n");

			float[] consumes = new float[6];

			var e = mLoadProfile.GetEnumerator();
			while (e.MoveNext())
			{
				var name	= e.Current.Key;
				var value	= e.Current.Value;

				//sb.AppendFormat("\tPath = {0}\n", name);
				for (int i = 0; i < value.Length; ++ i)
				{
					consumes[i] += value[i];

					//sb.AppendFormat("\t\tStep[{0}] = {1}\n", mLoadStepName[i], value[i]);
				}
			}

			for (int i = 0; i < 6; ++ i)
				sb.AppendFormat("\tTotal step[{0}] consume = {1}\n", mLoadStepName[i], consumes[i]);

			info = sb.ToString();
		}
	}
}
