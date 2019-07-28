
using System;
using System.Diagnostics;

namespace UEngine
{
    internal abstract class ITimerData
    {
        private uint mTimerID;
        public uint TimerID
        {
            get { return mTimerID; }
            set { mTimerID = value; }
        }

        private int mInterval;
        public int Interval
        {
            get { return mInterval; }
            set { mInterval = value; }
        }

        private ulong mNextTick;
        public ulong NextTick
        {
            get { return mNextTick; }
            set { mNextTick = value; }
        }

        public abstract Delegate Action
        {
            get;
            set;
        }

        public abstract void DoAction();
    }

    internal class UTimerData : ITimerData
    {
        private Action mAction;

        public override Delegate Action
        {
            get { return mAction; }
            set { mAction = value as Action; }
        }

        public override void DoAction()
        {
            mAction();
        }
    }

    internal class UTimerData< T > : ITimerData
    {
        private Action< T > mAction;

        public override Delegate Action
        {
            get { return mAction; }
            set { mAction = value as Action< T >; }
        }

        private T mArg1;
        public T Arg1
        {
            get { return mArg1; }
            set { mArg1 = value; }
        }

        public override void DoAction()
        {
            mAction(mArg1);
        }
    }

    internal class UTimerData< T, U > : ITimerData
    {
        private Action< T, U > mAction;

        public override Delegate Action
        {
            get { return mAction; }
            set { mAction = value as Action< T, U >; }
        }

        private T mArg1;
        public T Arg1
        {
            get { return mArg1; }
            set { mArg1 = value; }
        }

        private U mArg2;
        public U Arg2
        {
            get { return mArg2; }
            set { mArg2 = value; }
        }

        public override void DoAction()
        {
            mAction(mArg1, mArg2);
        }
    }

    internal class UTimerData< T, U, V > : ITimerData
    {
        private Action< T, U, V > mAction;

        public override Delegate Action
        {
            get { return mAction; }
            set { mAction = value as Action< T, U, V >; }
        }

        private T mArg1;
        public T Arg1
        {
            get { return mArg1; }
            set { mArg1 = value; }
        }

        private U mArg2;
        public U Arg2
        {
            get { return mArg2; }
            set { mArg2 = value; }
        }

        private V mArg3;

        public V Arg3
        {
            get { return mArg3; }
            set { mArg3 = value; }
        }

        public override void DoAction()
        {
            mAction(mArg1, mArg2, mArg3);
        }
    }

	internal class UTimerData< T, U, V, W > : ITimerData
	{
		private Action< T, U, V, W > mAction;

		public override Delegate Action
		{
			get { return mAction; }
			set { mAction = value as Action< T, U, V, W >; }
		}

		private T mArg1;
		public T Arg1
		{
			get { return mArg1; }
			set { mArg1 = value; }
		}

		private U mArg2;
		public U Arg2
		{
			get { return mArg2; }
			set { mArg2 = value; }
		}

		private V mArg3;

		public V Arg3
		{
			get { return mArg3; }
			set { mArg3 = value; }
		}

		private W mArg4;
		public W Arg4
		{
			get { return mArg4; }
			set { mArg4 = value; }
		}

		public override void DoAction()
		{
			mAction(mArg1, mArg2, mArg3, mArg4);
		}
	}

    public class UTimer
    {
        private static uint mNextTimerId;
        private static uint mUnTick;
        private static UKeyedPriorityQueue< uint, ITimerData, ulong > mQueue;
        private static Stopwatch mStopWatch;
        private static readonly object mQueueLock = new object();

        private UTimer()
        {
        }

        static UTimer()
        {
            mQueue = new UKeyedPriorityQueue< uint, ITimerData, ulong >();

            mStopWatch = new Stopwatch();
        }

        public static uint AddTimer(uint start, int interval, Action handler)
        {
            var p = GetTimerData(new UTimerData(), start, interval);
                p.Action = handler;

            return AddTimer(p);
        }

        public static uint AddTimer< T >(uint start, int interval, Action< T > handler, T arg1)
        {
            var p = GetTimerData(new UTimerData< T >(), start, interval);
                p.Action = handler;
                p.Arg1 = arg1;

            return AddTimer(p);
        }

        public static uint AddTimer< T, U >(uint start, int interval, Action< T, U > handler, T arg1, U arg2)
        {
            var p = GetTimerData(new UTimerData< T, U >(), start, interval);
                p.Action = handler;
                p.Arg1 = arg1;
                p.Arg2 = arg2;

            return AddTimer(p);
        }

        public static uint AddTimer< T, U, V >(uint start, int interval, Action< T, U, V > handler, T arg1, U arg2, V arg3)
        {
            var p = GetTimerData(new UTimerData< T, U, V >(), start, interval);
                p.Action = handler;
                p.Arg1 = arg1;
                p.Arg2 = arg2;
                p.Arg3 = arg3;

            return AddTimer(p);
        }

		public static uint AddTimer< T, U, V, W >(uint start, int interval, Action< T, U, V, W > handler, T arg1, U arg2, V arg3, W arg4)
		{
			var p = GetTimerData(new UTimerData< T, U, V, W >(), start, interval);
				p.Action = handler;
				p.Arg1 = arg1;
				p.Arg2 = arg2;
				p.Arg3 = arg3;
				p.Arg4 = arg4;

			return AddTimer(p);
		}

        public static void DelTimer(uint timerId)
        {
            lock (mQueueLock)
                mQueue.Remove(timerId);
        }

        public static void Tick()
        {
            mUnTick += (uint)mStopWatch.ElapsedMilliseconds;

            mStopWatch.Reset();
            mStopWatch.Start();

            while (mQueue.Count != 0)
            {
                ITimerData p;
                lock (mQueueLock)
                    p = mQueue.Peek();
                if (mUnTick < p.NextTick)
                    break;

                lock (mQueueLock)
                    mQueue.Dequeue();
                if (p.Interval > 0)
                {
                    p.NextTick += (ulong)p.Interval;
                    lock (mQueueLock)
                        mQueue.Enqueue(p.TimerID, p, p.NextTick);

                    p.DoAction();
                } else
                {
                    p.DoAction();
                }
            }
        }

        public static void Reset()
        {
            mUnTick = 0;
            mNextTimerId = 0;
            lock (mQueueLock)
            {
                while (mQueue.Count != 0)
                    mQueue.Dequeue();
            }
        }

        private static uint AddTimer(ITimerData p)
        {
            lock (mQueueLock)
                mQueue.Enqueue(p.TimerID, p, p.NextTick);

            return p.TimerID;
        }

        private static T GetTimerData< T >(T p, uint start, int interval) where T : ITimerData
        {
            p.Interval = interval;
            p.TimerID = ++ mNextTimerId;
            p.NextTick = mUnTick + 1 + start;

            return p;
        }
    }
}
