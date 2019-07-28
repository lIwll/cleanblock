using System;
using System.Collections.Generic;

namespace UEngine
{
    internal abstract class ICmd
    {
        public abstract Delegate Action
        {
            get;
            set;
        }

        public abstract void DoAction();
    }

    internal class UCmd: ICmd
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

    internal class UCmd< T > : ICmd
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

    internal class UCmd< T, U > : ICmd
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

    internal class UCmd< T, U, V > : ICmd
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

    public class UCmdDispatcher
    {
        private static Queue< ICmd > mCmdQueue;
        private static readonly object mQueueLock = new object();

        private UCmdDispatcher()
        {
        }

        static UCmdDispatcher()
        {
            mCmdQueue = new Queue< ICmd >();
        }

        public static void AddCmd(Action handler)
        {
            var p = new UCmd();
                p.Action = handler;

            AddCmd(p);
        }

        public static void AddCmd< T >(Action< T > handler, T arg1)
        {
            var p = new UCmd< T >();
                p.Action = handler;
                p.Arg1 = arg1;

            AddCmd(p);
        }

        public static void AddCmd< T, U >(Action< T, U > handler, T arg1, U arg2)
        {
            var p = new UCmd< T, U >();
                p.Action = handler;
                p.Arg1 = arg1;
                p.Arg2 = arg2;

            AddCmd(p);
        }

        public static void AddCmd< T, U, V >(Action< T, U, V > handler, T arg1, U arg2, V arg3)
        {
            var p = new UCmd< T, U, V >();
                p.Action = handler;
                p.Arg1 = arg1;
                p.Arg2 = arg2;
                p.Arg3 = arg3;

            AddCmd(p);
        }

        public static void Tick()
        {
            if (mCmdQueue.Count > 0)
            {
                ICmd p;
                {
                    lock (mQueueLock)
                        p = mCmdQueue.Peek();
                }

                {
                    lock (mQueueLock)
                        mCmdQueue.Dequeue();
                }

                p.DoAction();
            }
        }

        public static void Reset()
        {
            lock (mQueueLock)
                mCmdQueue.Clear();
        }

        private static void AddCmd(ICmd p)
        {
            lock (mQueueLock)
                mCmdQueue.Enqueue(p);
        }
    }
}
